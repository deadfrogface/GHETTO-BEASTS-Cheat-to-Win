using System;
using System.Collections.Generic;
using System.Linq;

namespace GhettoBeasts.Core
{
    /// <summary>Simple street progression: win fights → unlock buildings / back alley block.</summary>
    public static class StreetProgression
    {
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
    }
}
