using System;

namespace GhettoBeasts.Data
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Boss,
        /// <summary>Darknet glitch tier — ultra-rare corrupted DNA pulls.</summary>
        GenCorrupted
    }

    public enum CardKind
    {
        Beast,
        Npc
    }

    public enum MutationSlot
    {
        Head,
        Limbs,
        Skin
    }

    [Flags]
    public enum CardTraits
    {
        None = 0,
        Biological = 1,
        Cyber = 2,
        Regenerates = 4,
        Shielded = 8
    }

    public enum AbilityId
    {
        ToxicSpit,
        JawLock,
        Frenzy,
        MadLaugh,
        DeathRoll,
        ChestPound,
        LaserEyes,
        AcidSpray,
        DirtyNeedle,
        WrenchTuning,
        Seduction,
        HouseBan,
        BackalleySurgery,
        DeathDance,
        CallBodyguards
    }

    public enum MutationEffect
    {
        None,
        BleedOnHit,
        TrueStrike,
        CritDamage,
        PoisonOnHit,
        ShockCounter,
        CamouflageFirstRound
    }

    public static class RarityExtensions
    {
        public static string DisplayName(this Rarity rarity) => rarity switch
        {
            Rarity.Common => "Common",
            Rarity.Uncommon => "Uncommon",
            Rarity.Rare => "Rare",
            Rarity.Epic => "Epic",
            Rarity.Legendary => "Legendary",
            Rarity.Boss => "Boss",
            Rarity.GenCorrupted => "Gen-Corrupted",
            _ => rarity.ToString()
        };

        public static string DisplayName(this MutationSlot slot) => slot switch
        {
            MutationSlot.Head => "Kopf",
            MutationSlot.Limbs => "Gliedmaßen",
            MutationSlot.Skin => "Haut/Panzer",
            _ => slot.ToString()
        };
    }
}
