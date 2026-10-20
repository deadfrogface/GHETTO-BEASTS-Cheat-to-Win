using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Godot;

namespace GhettoBeasts.Visuals
{
    public static class Palette
    {
        public static readonly Color NeonPink = new(1f, 0.18f, 0.66f);
        public static readonly Color NeonCyan = new(0.1f, 0.95f, 1f);
        public static readonly Color NeonPurple = new(0.62f, 0.3f, 1f);
        public static readonly Color NeonGreen = new(0.25f, 1f, 0.5f);
        public static readonly Color NeonYellow = new(1f, 0.85f, 0.2f);
        public static readonly Color Danger = new(1f, 0.22f, 0.3f);
        public static readonly Color PoliceBlue = new(0.15f, 0.35f, 1f);
        public static readonly Color DirtyCash = new(0.55f, 0.85f, 0.35f);
        public static readonly Color CleanCash = new(0.85f, 0.95f, 1f);
        public static readonly Color Diamond = new(0.45f, 0.9f, 1f);
        public static readonly Color Background = new(0.03f, 0.015f, 0.06f);
        public static readonly Color PanelBg = new(0.05f, 0.03f, 0.1f, 0.88f);
        public static readonly Color TextDim = new(0.7f, 0.66f, 0.82f);

        public static Color ForRarity(Rarity rarity) => rarity switch
        {
            Rarity.Common => new Color(0.68f, 0.7f, 0.74f),
            Rarity.Uncommon => new Color(0.22f, 1f, 0.53f),
            Rarity.Rare => new Color(0.2f, 0.7f, 1f),
            Rarity.Epic => new Color(0.7f, 0.3f, 1f),
            Rarity.Legendary => new Color(1f, 0.76f, 0.2f),
            Rarity.Boss => new Color(1f, 0.2f, 0.33f),
            _ => Colors.White
        };

        public static Color ForStatus(StatusType status) => status switch
        {
            StatusType.Bleed => new Color(0.9f, 0.08f, 0.15f),
            StatusType.Poison => new Color(0.6f, 0.95f, 0.15f),
            StatusType.Confused => NeonPink,
            StatusType.Stunned => NeonYellow,
            StatusType.Banished => new Color(0.6f, 0.6f, 0.7f),
            StatusType.Doped => NeonGreen,
            StatusType.Enraged => Danger,
            _ => Colors.White
        };

        public static string StatusName(StatusType status) => status switch
        {
            StatusType.Bleed => "Blutung",
            StatusType.Poison => "Gift",
            StatusType.Confused => "Verwirrt",
            StatusType.Stunned => "Betäubt",
            StatusType.Banished => "Hausverbot",
            StatusType.Doped => "Gedopt",
            StatusType.Enraged => "Zorn",
            _ => status.ToString()
        };
    }
}
