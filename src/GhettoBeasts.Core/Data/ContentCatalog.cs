using System;
using System.Collections.Generic;
using System.Linq;

namespace GhettoBeasts.Data
{
    public static partial class ContentCatalog
    {
        public const string CategoryRodents = "Urbane Nager & Parasiten";
        public const string CategoryPredators = "Hinterhof-Raubtiere & Kampfhunde";
        public const string CategoryExotic = "Exotische Schmuggeltiere & Primaten";
        public const string CategoryExperiments = "Cyber- & Gen-Experimente";
        public const string CategoryCrooks = "Straßen-Gauner & Kleinkriminelle";
        public const string CategoryClubStaff = "Club-Staff & Unterwelt-Personal";
        public const string CategoryBouncers = "Türsteher & Schwerverbrecher";
        public const string CategoryBosses = "Syndikat-Bosse & Kartell-Legenden";

        public const int TargetBeastCount = 50;
        public const int TargetNpcCount = 50;

        private static readonly IReadOnlyList<CardDefinition> HeroBeasts = new List<CardDefinition>
        {
            new("kanalratte", "Räudige Kanalratte", CardKind.Beast, Rarity.Common, CategoryRodents,
                new Stats(95, 42, 15, 80, 10, 8),
                AbilityId.ToxicSpit, "Giftspeichel", "Schwacher Biss, vergiftet das Ziel für 3 Runden.",
                "Schnell, hoher Giftspeichel-Schaden, aber wenig Lebenspunkte."),
            new("kupferstecher_pitbull", "Kupferstecher-Pitbull", CardKind.Beast, Rarity.Uncommon, CategoryPredators,
                new Stats(150, 50, 45, 45, 5, 5),
                AbilityId.JawLock, "Kiefersperre", "Verbeißt sich im Gegner und blockiert dessen nächste Aktion.",
                "Hohe Verteidigung, blockiert gegnerische Aktionen."),
            new("tiefgaragen_dobermann", "Tiefgaragen-Dobermann", CardKind.Beast, Rarity.Uncommon, CategoryPredators,
                new Stats(130, 62, 28, 75, 8, 10),
                AbilityId.Frenzy, "Raserei", "Zwei schnelle Bisse hintereinander.",
                "Aggressiver Angreifer mit hoher Initiative."),
            new("hyaenen_mischling", "Illegaler Hyänen-Mischling", CardKind.Beast, Rarity.Rare, CategoryPredators,
                new Stats(140, 55, 30, 65, 12, 8),
                AbilityId.MadLaugh, "Irres Lachen", "Verwirrt den Gegner für 2 Runden und klaut 40 $ Schmutzgeld.",
                "Verursacht Verwirrung und stiehlt Schmutzgeld beim Angriff."),
            new("kanal_alligator", "Kanalisations-Alligator", CardKind.Beast, Rarity.Rare, CategoryExotic,
                new Stats(190, 58, 65, 30, 0, 5),
                AbilityId.DeathRoll, "Todesrolle", "Zieht den Gegner unter Wasser: schwerer Treffer, Ziel ist 1 Runde betäubt.",
                "Extremer Panzerschutz, zieht Gegner unter Wasser."),
            new("silberruecken_gorilla", "Alpha-Silberrücken-Gorilla", CardKind.Beast, Rarity.Epic, CategoryExotic,
                new Stats(220, 72, 50, 40, 5, 8),
                AbilityId.ChestPound, "Brusttrommeln", "Wuchtiger Flächenschlag mit 60% Betäubungs-Chance.",
                "Hoher Flächenschaden, teilt Betäubungs-Effekte aus."),
            new("cyber_pit", "Cyber-Pitbull 'CYBER-PIT'", CardKind.Beast, Rarity.Legendary, CategoryExperiments,
                new Stats(230, 80, 70, 60, 5, 12),
                AbilityId.LaserEyes, "Laser-Augen", "Präziser Laserstrahl, der nie verfehlt.",
                "Titan-Legierung, Schildgeneratoren und Laser-Augen.",
                CardTraits.Cyber | CardTraits.Shielded, 10),
            new("crispr_01", "Giga-Kakerlake 'CRISPR-01'", CardKind.Beast, Rarity.Legendary, CategoryExperiments,
                new Stats(200, 60, 55, 50, 10, 5),
                AbilityId.AcidSpray, "Säure-Sprühregen", "Ätzende Säure: Schaden und garantierte Vergiftung.",
                "Nahezu unsterblich, regeneriert jede Runde und verteilt Säure.",
                CardTraits.Biological | CardTraits.Regenerates, 20)
        };

        private static readonly IReadOnlyList<CardDefinition> HeroNpcs = new List<CardDefinition>
        {
            new("hinterhof_crackhead", "Hinterhof-Crackhead", CardKind.Npc, Rarity.Common, CategoryCrooks,
                new Stats(100, 48, 15, 60, 10, 10),
                AbilityId.DirtyNeedle, "Gebrauchte Nadel", "Völlig unberechenbarer Stich (30-160% Schaden), kann vergiften.",
                "Greift mit gebrauchten Nadeln an, unberechenbares Angriffsmuster."),
            new("strassen_schrauber", "Illegaler Straßen-Schrauber", CardKind.Npc, Rarity.Common, CategoryClubStaff,
                new Stats(115, 40, 30, 50, 5, 5),
                AbilityId.WrenchTuning, "Schrauber-Tuning", "Repariert sich selbst (Cyber-Einheiten stärker) und boostet den Angriff.",
                "Repariert Cyber-Bestien im Kampf und boosted Werte."),
            new("chantal", "Club-Hostess 'Chantal'", CardKind.Npc, Rarity.Uncommon, CategoryClubStaff,
                new Stats(110, 40, 22, 70, 15, 8),
                AbilityId.Seduction, "Shirt hochziehen (Betörung)", "Verwirrt das gegnerische Monster für 2 Runden.",
                "Verwirrt Gegner, sodass sie sich selbst angreifen oder den Zug überspringen."),
            new("ede", "Korrupter Türsteher 'Ede'", CardKind.Npc, Rarity.Rare, CategoryBouncers,
                new Stats(200, 55, 60, 35, 0, 5),
                AbilityId.HouseBan, "Hausverbot", "Schlägt die gegnerische Karte zurück auf die Hand: setzt 1 Runde aus, Buffs weg.",
                "Schlägt gegnerische Summons direkt zurück auf die Hand."),
            new("dr_mutation", "Schwarzmarkt-Chirurg 'Dr. Mutation'", CardKind.Npc, Rarity.Epic, CategoryClubStaff,
                new Stats(150, 45, 40, 50, 5, 5),
                AbilityId.BackalleySurgery, "Hinterhof-OP", "Heilt 35% HP, entfernt Gift/Blutung und senkt Instabilität um 25.",
                "Heilt biologische Wesen im Kampf und entfernt Instabilität."),
            new("lilith", "Schatten-Stripperin 'Lilith'", CardKind.Npc, Rarity.Legendary, CategoryBosses,
                new Stats(170, 70, 30, 85, 30, 15),
                AbilityId.DeathDance, "Tödlicher Tanz", "Garantierter kritischer Treffer mit 3 Runden Blutung.",
                "Hohe Ausweichchance, verteilt kritische Blutungstreffer."),
            new("el_capo", "Kartell-Boss 'El Capo'", CardKind.Npc, Rarity.Boss, CategoryBosses,
                new Stats(280, 70, 55, 45, 5, 10),
                AbilityId.CallBodyguards, "Leibwächter rufen", "Zwei Leibwächter-Salven; besticht nebenbei das SWAT-Team (-25 Verdacht).",
                "Ruft bewaffnete Leibwächter und besticht das SWAT-Team im laufenden Kampf.")
        };

        public static IReadOnlyList<CardDefinition> Beasts { get; } = BuildStreetBeasts(HeroBeasts, TargetBeastCount);
        public static IReadOnlyList<CardDefinition> Npcs { get; } = BuildStreetNpcs(HeroNpcs, TargetNpcCount);

        public static IReadOnlyList<MutationDefinition> Mutations { get; } = new List<MutationDefinition>
        {
            new("gorilla_kiefer", "Gorilla-Kiefer", MutationSlot.Head,
                new Stats(0, 15, 0, 0, 0, 0), 15, MutationEffect.None,
                "+15 Angriffskraft, erhöht den physischen Schaden."),
            new("piranha_gebiss", "Piranha-Gebiss", MutationSlot.Head,
                new Stats(0, 5, 0, 0, 0, 0), 20, MutationEffect.BleedOnHit,
                "Treffer verursachen 2 Runden Blutung."),
            new("fledermaus_sonar", "Fledermaus-Sonarsystem", MutationSlot.Head,
                Stats.Zero, 15, MutationEffect.TrueStrike,
                "Trefferchance 100%, umgeht Ausweichen."),
            new("wolfs_nase", "Wolfs-Nase", MutationSlot.Head,
                new Stats(0, 0, 0, 10, 5, 0), 10, MutationEffect.None,
                "+10 Initiative und +5% Ausweichen."),
            new("stier_hoerner", "Stier-Hörner", MutationSlot.Head,
                new Stats(10, 10, 0, 0, 0, 5), 18, MutationEffect.None,
                "+10 HP, +10 Angriff, +5% Krit."),
            new("klingen_pranken", "Klingen-Pranken", MutationSlot.Limbs,
                new Stats(0, 22, 0, 0, 0, 10), 30, MutationEffect.CritDamage,
                "+22 Angriffskraft, extrem hoher kritischer Schaden, aber stark instabil."),
            new("kaenguru_sprungbeine", "Känguru-Sprungbeine", MutationSlot.Limbs,
                new Stats(0, 0, 0, 20, 15, 0), 15, MutationEffect.None,
                "+15% Ausweichen und +20 Initiative."),
            new("skorpion_stachel", "Skorpion-Stachel", MutationSlot.Limbs,
                Stats.Zero, 20, MutationEffect.PoisonOnHit,
                "Treffer injizieren Nervengift für 3 Runden."),
            new("krabben_scheren", "Krabben-Scheren", MutationSlot.Limbs,
                new Stats(0, 12, 8, 0, 0, 0), 16, MutationEffect.None,
                "+12 Angriff und +8 Verteidigung."),
            new("spinne_beine", "Spinnen-Beine", MutationSlot.Limbs,
                new Stats(0, 0, 0, 15, 20, 0), 22, MutationEffect.None,
                "+20% Ausweichen und +15 Initiative."),
            new("chitin_panzer", "Chitin-Panzer", MutationSlot.Skin,
                new Stats(0, 0, 20, 0, 0, 0), 15, MutationEffect.None,
                "+20 Verteidigung, reduziert physischen Schaden."),
            new("aal_strom_haut", "Aal-Strom-Haut", MutationSlot.Skin,
                Stats.Zero, 20, MutationEffect.ShockCounter,
                "Teilt Elektroschock-Konterschaden aus, wenn das Biest getroffen wird."),
            new("chamaeleon_tarnhaut", "Chamäleon-Tarnhaut", MutationSlot.Skin,
                Stats.Zero, 20, MutationEffect.CamouflageFirstRound,
                "In der ersten Kampfrunde unsichtbar für Angriffe."),
            new("titan_platten", "Titan-Platten", MutationSlot.Skin,
                new Stats(30, 0, 25, -5, 0, 0), 25, MutationEffect.None,
                "+30 HP, +25 Verteidigung, aber -5 Initiative."),
            new("neon_adern", "Neon-Adern", MutationSlot.Skin,
                new Stats(0, 8, 0, 8, 0, 8), 18, MutationEffect.None,
                "+8 Angriff, Initiative und Krit – leuchtet wie Gift.")
        };

        public static IEnumerable<CardDefinition> AllCards => Beasts.Concat(Npcs);
        public static IReadOnlyList<CardDefinition> Heroes => HeroBeasts.Concat(HeroNpcs).ToList();

        public static CardDefinition GetCard(string id) =>
            AllCards.FirstOrDefault(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Unbekannte Karte '{id}'.");

        public static bool TryGetCard(string id, out CardDefinition card)
        {
            card = AllCards.FirstOrDefault(c => c.Id == id)!;
            return card is not null;
        }

        public static MutationDefinition GetMutation(string id) =>
            Mutations.FirstOrDefault(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Unbekannte Mutation '{id}'.");

        public static IEnumerable<MutationDefinition> MutationsForSlot(MutationSlot slot) =>
            Mutations.Where(m => m.Slot == slot);

        public static int DirtyCashReward(Rarity rarity) => rarity switch
        {
            Rarity.Common => 50,
            Rarity.Uncommon => 80,
            Rarity.Rare => 130,
            Rarity.Epic => 200,
            Rarity.Legendary => 320,
            Rarity.Boss => 500,
            _ => throw new ArgumentOutOfRangeException(nameof(rarity))
        };
    }
}
