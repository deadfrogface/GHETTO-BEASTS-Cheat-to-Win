using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Data;

namespace GhettoBeasts.Core
{
    /// <summary>Street campaign tier derived from win count (aligns with quest unlock thresholds).</summary>
    public enum StreetTier
    {
        Rookie = 0,
        DopeRun = 1,
        Armed = 2,
        BackAlley = 3,
        Dock = 4
    }

    /// <summary>Street progression: win fights → unlock buildings / blocks → harder encounter tiers.</summary>
    public static class StreetProgression
    {
        /// <summary>Bump this when shipping a visible street update (shows on district HUD).</summary>
        public const string CampaignTag = "STREET-v2 LIVE";

        public const string Arena = "arena";
        public const string Crispr = "crispr";
        public const string Wash = "wash";
        public const string Darknet = "darknet";
        public const string Dope = "dope";
        public const string Weap = "weap";
        public const string BackAlley = "back_alley";
        public const string DockBlock = "dock_block";

        public static readonly string[] AlwaysUnlocked =
        {
            Arena, Crispr, Wash, Darknet
        };

        public sealed record QuestGoal(string Id, string Title, string Hint, int TargetWins, string UnlockId);

        public static readonly QuestGoal[] Quests =
        {
            new("q_dope", "Dope-Laden knacken", "Gewinne 2 Straßenkämpfe.", 2, Dope),
            new("q_weap", "Waffenschuppen", "Gewinne 3 Straßenkämpfe.", 3, Weap),
            new("q_back", "Hintergasse öffnen", "Gewinne 5 Kämpfe — neuer Block.", 5, BackAlley),
            new("q_dock", "Dock-Block", "Gewinne 8 Kämpfe — Trainer & Beute.", 8, DockBlock)
        };

        /// <summary>Milestone NPC bosses tied to unlocked street tiers (hero catalog ids).</summary>
        public static readonly IReadOnlyDictionary<StreetTier, string> MilestoneBossByTier =
            new Dictionary<StreetTier, string>
            {
                [StreetTier.DopeRun] = "ede",
                [StreetTier.Armed] = "dr_mutation",
                [StreetTier.BackAlley] = "lilith",
                [StreetTier.Dock] = "el_capo"
            };

        public static void EnsureDefaults(SaveData save)
        {
            save.UnlockedBuildingIds ??= new List<string>();
            save.CompletedQuestIds ??= new List<string>();
            foreach (var id in AlwaysUnlocked)
            {
                if (!save.UnlockedBuildingIds.Contains(id))
                    save.UnlockedBuildingIds.Add(id);
            }
            SyncUnlocksFromWins(save);
        }

        public static bool IsUnlocked(SaveData save, string buildingId)
        {
            EnsureDefaults(save);
            return save.UnlockedBuildingIds.Contains(buildingId);
        }

        public static void RegisterWin(SaveData save)
        {
            EnsureDefaults(save);
            save.StreetWins++;
            SyncUnlocksFromWins(save);
        }

        public static void SyncUnlocksFromWins(SaveData save)
        {
            foreach (var quest in Quests)
            {
                if (save.StreetWins < quest.TargetWins) continue;
                if (!save.UnlockedBuildingIds.Contains(quest.UnlockId))
                    save.UnlockedBuildingIds.Add(quest.UnlockId);
                if (!save.CompletedQuestIds.Contains(quest.Id))
                    save.CompletedQuestIds.Add(quest.Id);
            }
        }

        public static IEnumerable<(QuestGoal Quest, bool Done, int Progress)> ListQuests(SaveData save)
        {
            EnsureDefaults(save);
            foreach (var q in Quests)
            {
                bool done = save.CompletedQuestIds.Contains(q.Id) || IsUnlocked(save, q.UnlockId);
                int progress = Math.Min(save.StreetWins, q.TargetWins);
                yield return (q, done, progress);
            }
        }

        public static string NextHint(SaveData save)
        {
            var next = ListQuests(save).FirstOrDefault(x => !x.Done);
            if (next.Quest is null) return "Alle Blöcke offen. Crush the street.";
            return $"{next.Quest.Title}: {next.Progress}/{next.Quest.TargetWins} — {next.Quest.Hint}";
        }

        public static StreetTier GetStreetTier(SaveData save) => GetStreetTier(save.StreetWins);

        public static StreetTier GetStreetTier(int streetWins) => streetWins switch
        {
            >= 8 => StreetTier.Dock,
            >= 5 => StreetTier.BackAlley,
            >= 3 => StreetTier.Armed,
            >= 2 => StreetTier.DopeRun,
            _ => StreetTier.Rookie
        };

        public static string TierLabel(StreetTier tier) => tier switch
        {
            StreetTier.Rookie => "Tier 0 · Rookie Street",
            StreetTier.DopeRun => "Tier 1 · Dope Run",
            StreetTier.Armed => "Tier 2 · Armed Block",
            StreetTier.BackAlley => "Tier 3 · Back Alley",
            StreetTier.Dock => "Tier 4 · Dock Lords",
            _ => "Tier ?"
        };

        public static string StatusLine(SaveData save) =>
            $"{TierLabel(GetStreetTier(save))} · {NextHint(save)}";

        /// <summary>Inclusive rarity band for wild beast spawns at this tier.</summary>
        public static (Rarity Min, Rarity Max) WildRarityBand(StreetTier tier) => tier switch
        {
            StreetTier.Rookie => (Rarity.Common, Rarity.Uncommon),
            StreetTier.DopeRun => (Rarity.Common, Rarity.Rare),
            StreetTier.Armed => (Rarity.Uncommon, Rarity.Rare),
            StreetTier.BackAlley => (Rarity.Uncommon, Rarity.Epic),
            StreetTier.Dock => (Rarity.Rare, Rarity.Legendary),
            _ => (Rarity.Common, Rarity.Uncommon)
        };

        /// <summary>Inclusive rarity band for street trainer spawns at this tier.</summary>
        public static (Rarity Min, Rarity Max) TrainerRarityBand(StreetTier tier) => tier switch
        {
            StreetTier.Rookie => (Rarity.Common, Rarity.Rare),
            StreetTier.DopeRun => (Rarity.Common, Rarity.Rare),
            StreetTier.Armed => (Rarity.Uncommon, Rarity.Epic),
            StreetTier.BackAlley => (Rarity.Rare, Rarity.Legendary),
            StreetTier.Dock => (Rarity.Epic, Rarity.Boss),
            _ => (Rarity.Common, Rarity.Rare)
        };

        /// <summary>
        /// Highest milestone boss unlocked by street wins (null on Rookie).
        /// Spawned on one trainer pad so the campaign has a clear apex fight.
        /// </summary>
        public static string? MilestoneBossId(SaveData save)
        {
            EnsureDefaults(save);
            var tier = GetStreetTier(save);
            string? best = null;
            foreach (var kv in MilestoneBossByTier.OrderBy(k => (int)k.Key))
            {
                if ((int)tier >= (int)kv.Key)
                    best = kv.Value;
            }
            return best;
        }

        public static List<string> BuildEncounterPool(CardKind kind, Rarity min, Rarity max, string fallbackId)
        {
            if (min > max) (min, max) = (max, min);
            var ids = ContentCatalog.AllCards
                .Where(c =>
                    c.Kind == kind &&
                    c.Rarity != Rarity.GenCorrupted &&
                    c.Rarity >= min &&
                    c.Rarity <= max)
                .Select(c => c.Id)
                .ToList();
            if (ids.Count == 0)
                ids.Add(fallbackId);
            return ids;
        }

        public static List<string> BuildWildPool(SaveData save)
        {
            var (min, max) = WildRarityBand(GetStreetTier(save));
            return BuildEncounterPool(CardKind.Beast, min, max, "kanalratte");
        }

        public static List<string> BuildTrainerPool(SaveData save)
        {
            var (min, max) = TrainerRarityBand(GetStreetTier(save));
            return BuildEncounterPool(CardKind.Npc, min, max, "chantal");
        }

        public static string PickFromPool(IReadOnlyList<string> pool, IRandom random)
        {
            if (pool is null || pool.Count == 0) return "kanalratte";
            return pool[random.Range(0, pool.Count)];
        }
    }
}
