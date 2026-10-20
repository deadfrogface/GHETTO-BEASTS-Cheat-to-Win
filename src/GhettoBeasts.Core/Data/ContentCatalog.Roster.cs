using System;
using System.Collections.Generic;
using System.Linq;

namespace GhettoBeasts.Data
{
    /// <summary>
    /// Expands the roster to 50+ beasts and 50+ NPCs via seeded archetype factories.
    /// The 15 named hero units from the design doc stay first; street variants fill the rest.
    /// </summary>
    public static partial class ContentCatalog
    {
        private static string[] Districts() => new[]
        {
            "Kanal", "Hinterhof", "Tiefgarage", "Container", "Neon", "Asphalt", "Keller", "Dach",
            "Parkhaus", "Müllkippe", "U-Bahn", "Hafen", "Dock", "Tunnel", "Allee", "Gasse",
            "Club", "Bunker", "Hof", "Rampe", "Schrottplatz", "Werkstatt", "Markt", "Sumpf",
            "Brücke"
        };

        private static (string Id, string Name, string Category, AbilityId Ability, string AbilityName, string AbilityDesc, CardTraits Traits, BodyArchetype Archetype)[] BeastArchetypes() =>
        new[]
        {
            ("ratte", "Ratte", CategoryRodents, AbilityId.ToxicSpit, "Giftspeichel", "Vergiftet das Ziel für 3 Runden.", CardTraits.Biological, BodyArchetype.Rodent),
            ("kakerlake", "Kakerlake", CategoryRodents, AbilityId.AcidSpray, "Säure-Biss", "Ätzender Biss mit Gift.", CardTraits.Biological | CardTraits.Regenerates, BodyArchetype.Insect),
            ("marder", "Marder", CategoryRodents, AbilityId.Frenzy, "Blitzraub", "Zwei schnelle Hiebe.", CardTraits.Biological, BodyArchetype.Rodent),
            ("streuner", "Streuner", CategoryPredators, AbilityId.Frenzy, "Straßenbiss", "Doppelter Biss.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("pitbull", "Pitbull", CategoryPredators, AbilityId.JawLock, "Kiefersperre", "Blockiert die nächste Aktion.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("dobermann", "Dobermann", CategoryPredators, AbilityId.Frenzy, "Jagdinstinkt", "Zwei Angriffe.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("bulldogge", "Bulldogge", CategoryPredators, AbilityId.JawLock, "Eisenkiefer", "Betäubt das Ziel.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("hyaene", "Hyäne", CategoryPredators, AbilityId.MadLaugh, "Irres Lachen", "Verwirrt und klaut Cash.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("fuchs", "Fuchs", CategoryPredators, AbilityId.ToxicSpit, "Giftklaue", "Giftiger Streich.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("alligator", "Alligator", CategoryExotic, AbilityId.DeathRoll, "Todesrolle", "Schwerer Treffer + Betäubung.", CardTraits.Biological, BodyArchetype.Reptile),
            ("python", "Python", CategoryExotic, AbilityId.JawLock, "Würgeschlinge", "Blockiert den Gegner.", CardTraits.Biological, BodyArchetype.Reptile),
            ("affen", "Affe", CategoryExotic, AbilityId.ChestPound, "Trommeln", "Flächenstoß mit Betäubungs-Chance.", CardTraits.Biological, BodyArchetype.Primate),
            ("gorilla", "Gorilla", CategoryExotic, AbilityId.ChestPound, "Brusttrommeln", "Wuchtiger Schlag.", CardTraits.Biological, BodyArchetype.Primate),
            ("gepard", "Gepard", CategoryExotic, AbilityId.Frenzy, "Sprintbiss", "Zwei schnelle Treffer.", CardTraits.Biological, BodyArchetype.Quadruped),
            ("cyberhund", "Cyberhund", CategoryExperiments, AbilityId.LaserEyes, "Laser-Augen", "Trifft immer.", CardTraits.Cyber | CardTraits.Shielded, BodyArchetype.Quadruped),
            ("genratte", "Gen-Ratte", CategoryExperiments, AbilityId.AcidSpray, "Zell-Säure", "Säure und Gift.", CardTraits.Biological | CardTraits.Regenerates, BodyArchetype.Rodent),
            ("dronenkaefer", "Drohnenkäfer", CategoryExperiments, AbilityId.LaserEyes, "Scan-Laser", "Präziser Strahl.", CardTraits.Cyber, BodyArchetype.Insect),
            ("mutant", "Mutant", CategoryExperiments, AbilityId.AcidSpray, "Instabiler Ausbruch", "Säure-Regen.", CardTraits.Biological | CardTraits.Regenerates, BodyArchetype.Primate)
        };

        private static (string Id, string Name, string Category, AbilityId Ability, string AbilityName, string AbilityDesc)[] NpcArchetypes() =>
        new[]
        {
            ("crackhead", "Crackhead", CategoryCrooks, AbilityId.DirtyNeedle, "Gebrauchte Nadel", "Unberechenbarer Stich, Chance auf Gift."),
            ("dealer", "Dealer", CategoryCrooks, AbilityId.MadLaugh, "Fake-Deal", "Verwirrt und klaut Cash."),
            ("taschendieb", "Taschendieb", CategoryCrooks, AbilityId.Frenzy, "Doppel-Klau", "Zwei schnelle Treffer."),
            ("graffiti", "Graffiti-Sprayer", CategoryCrooks, AbilityId.ToxicSpit, "Farbnebel", "Giftiger Spray."),
            ("schrauber", "Schrauber", CategoryClubStaff, AbilityId.WrenchTuning, "Schrauber-Tuning", "Heilung und ATK-Boost."),
            ("hostess", "Hostess", CategoryClubStaff, AbilityId.Seduction, "Betörung", "Verwirrt für 2 Runden."),
            ("barkeeper", "Barkeeper", CategoryClubStaff, AbilityId.DirtyNeedle, "Knockout-Drink", "Unberechenbarer Treffer."),
            ("dj", "Club-DJ", CategoryClubStaff, AbilityId.MadLaugh, "Bass-Drop", "Verwirrt und klaut Cash."),
            ("tuersteher", "Türsteher", CategoryBouncers, AbilityId.HouseBan, "Hausverbot", "Banished 1 Runde."),
            ("enforcer", "Enforcer", CategoryBouncers, AbilityId.JawLock, "Knebelgriff", "Betäubt das Ziel."),
            ("bodyguard", "Bodyguard", CategoryBouncers, AbilityId.ChestPound, "Schutzwall", "Wuchtiger Schlag."),
            ("hitman", "Hitman", CategoryBouncers, AbilityId.DeathDance, "Silent Kill", "Kritischer Blutungstreffer."),
            ("chirurg", "Chirurg", CategoryClubStaff, AbilityId.BackalleySurgery, "Hinterhof-OP", "Heilt und senkt Instabilität."),
            ("stripper", "Stripperin", CategoryBosses, AbilityId.DeathDance, "Tödlicher Tanz", "Krit + Blutung."),
            ("boss", "Boss", CategoryBosses, AbilityId.CallBodyguards, "Leibwächter", "Doppeltreffer, senkt Verdacht."),
            ("capo", "Capo", CategoryBosses, AbilityId.CallBodyguards, "Kartell-Ruf", "Leibwächter + Bestechung.")
        };

        private static Rarity[] StreetRarities() => new[]
        {
            Rarity.Common, Rarity.Common, Rarity.Common, Rarity.Uncommon, Rarity.Uncommon,
            Rarity.Rare, Rarity.Rare, Rarity.Epic, Rarity.Legendary, Rarity.Boss
        };

        public enum BodyArchetype
        {
            Rodent,
            Quadruped,
            Reptile,
            Primate,
            Insect,
            Humanoid
        }

        public static BodyArchetype GetBodyArchetype(string cardId)
        {
            if (HeroBodyOverrides.TryGetValue(cardId, out var hero)) return hero;
            foreach (var archetype in BeastArchetypes())
            {
                if (cardId.EndsWith("_" + archetype.Id, StringComparison.Ordinal) || cardId == archetype.Id)
                    return archetype.Archetype;
            }
            return BodyArchetype.Humanoid;
        }

        private static readonly Dictionary<string, BodyArchetype> HeroBodyOverrides = new()
        {
            ["kanalratte"] = BodyArchetype.Rodent,
            ["kupferstecher_pitbull"] = BodyArchetype.Quadruped,
            ["tiefgaragen_dobermann"] = BodyArchetype.Quadruped,
            ["hyaenen_mischling"] = BodyArchetype.Quadruped,
            ["kanal_alligator"] = BodyArchetype.Reptile,
            ["silberruecken_gorilla"] = BodyArchetype.Primate,
            ["cyber_pit"] = BodyArchetype.Quadruped,
            ["crispr_01"] = BodyArchetype.Insect
        };

        internal static List<CardDefinition> BuildStreetBeasts(IReadOnlyList<CardDefinition> heroes, int targetCount)
        {
            var list = new List<CardDefinition>(heroes);
            var used = new HashSet<string>(heroes.Select(h => h.Id));
            int index = 0;
            var archetypes = BeastArchetypes();
            var districts = Districts();
            var rarities = StreetRarities();
            while (list.Count < targetCount)
            {
                var archetype = archetypes[index % archetypes.Length];
                var district = districts[index % districts.Length];
                var rarity = rarities[index % rarities.Length];
                if (rarity == Rarity.Boss) rarity = Rarity.Legendary;
                string id = $"{Slug(district)}_{archetype.Id}";
                if (!used.Add(id))
                {
                    id = $"{id}_{index}";
                    used.Add(id);
                }

                var stats = ScaleStats(BaseBeastStats(archetype.Archetype), rarity, index);
                int instability = archetype.Traits.HasFlag(CardTraits.Cyber) ? 8 + index % 12 : index % 10;
                list.Add(new CardDefinition(
                    id,
                    $"{district}-{archetype.Name}",
                    CardKind.Beast,
                    rarity,
                    archetype.Category,
                    stats,
                    archetype.Ability,
                    archetype.AbilityName,
                    archetype.AbilityDesc,
                    $"{district}-Variante: {archetype.Name} aus dem Viertel.",
                    archetype.Traits,
                    instability));
                index++;
            }
            return list;
        }

        internal static List<CardDefinition> BuildStreetNpcs(IReadOnlyList<CardDefinition> heroes, int targetCount)
        {
            var list = new List<CardDefinition>(heroes);
            var used = new HashSet<string>(heroes.Select(h => h.Id));
            var archetypes = NpcArchetypes();
            var districts = Districts();
            var rarities = StreetRarities();
            int index = 0;
            while (list.Count < targetCount)
            {
                var archetype = archetypes[index % archetypes.Length];
                var district = districts[(index * 3) % districts.Length];
                var rarity = rarities[index % rarities.Length];
                if (archetype.Category != CategoryBosses && rarity == Rarity.Boss) rarity = Rarity.Legendary;
                if (archetype.Category == CategoryBosses && rarity < Rarity.Epic) rarity = Rarity.Epic;

                string id = $"{Slug(district)}_{archetype.Id}";
                if (!used.Add(id))
                {
                    id = $"{id}_{index}";
                    used.Add(id);
                }

                var stats = ScaleStats(BaseNpcStats(archetype.Ability), rarity, index);
                list.Add(new CardDefinition(
                    id,
                    $"{district}-{archetype.Name}",
                    CardKind.Npc,
                    rarity,
                    archetype.Category,
                    stats,
                    archetype.Ability,
                    archetype.AbilityName,
                    archetype.AbilityDesc,
                    $"Straßen-Söldner aus {district}.",
                    CardTraits.Biological));
                index++;
            }
            return list;
        }

        private static Stats BaseBeastStats(BodyArchetype archetype) => archetype switch
        {
            BodyArchetype.Rodent => new Stats(90, 40, 14, 78, 12, 8),
            BodyArchetype.Insect => new Stats(110, 45, 30, 55, 10, 6),
            BodyArchetype.Reptile => new Stats(170, 55, 55, 28, 2, 5),
            BodyArchetype.Primate => new Stats(190, 65, 42, 42, 6, 8),
            _ => new Stats(140, 52, 35, 55, 7, 7)
        };

        private static Stats BaseNpcStats(AbilityId ability) => ability switch
        {
            AbilityId.HouseBan => new Stats(185, 52, 55, 35, 2, 5),
            AbilityId.CallBodyguards => new Stats(240, 68, 50, 42, 4, 8),
            AbilityId.DeathDance => new Stats(155, 68, 28, 80, 25, 14),
            AbilityId.BackalleySurgery => new Stats(140, 42, 38, 48, 5, 5),
            AbilityId.Seduction => new Stats(105, 38, 20, 68, 14, 8),
            AbilityId.WrenchTuning => new Stats(120, 40, 32, 48, 5, 5),
            _ => new Stats(110, 46, 20, 55, 8, 8)
        };

        private static Stats ScaleStats(Stats baseStats, Rarity rarity, int seed)
        {
            float mul = rarity switch
            {
                Rarity.Common => 0.95f,
                Rarity.Uncommon => 1.05f,
                Rarity.Rare => 1.15f,
                Rarity.Epic => 1.28f,
                Rarity.Legendary => 1.4f,
                Rarity.Boss => 1.55f,
                _ => 1f
            };
            int jitter = (seed % 5) - 2;
            return new Stats(
                Scale(baseStats.MaxHp, mul, jitter * 3),
                Scale(baseStats.Attack, mul, jitter),
                Scale(baseStats.Defense, mul, jitter),
                Scale(baseStats.Speed, mul, jitter),
                Math.Clamp(baseStats.Evasion + jitter, 0, 40),
                Math.Clamp(baseStats.CritChance + (jitter > 0 ? 1 : 0), 0, 25));
        }

        private static int Scale(int value, float mul, int jitter) =>
            Math.Max(1, (int)Math.Round(value * mul) + jitter);

        private static string Slug(string text) =>
            text.ToLowerInvariant()
                .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
                .Replace(" ", "_").Replace("-", "_");
    }
}
