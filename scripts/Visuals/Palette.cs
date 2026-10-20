using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>
    /// Muted cyberpunk-anime palette: desaturated bases, neon reserved for lights/UI accents.
    /// </summary>
    public static class Palette
    {
        // Accent neons (restrained — use sparingly on emissives / UI highlights)
        public static readonly Color NeonPink = new(0.82f, 0.38f, 0.58f);
        public static readonly Color NeonCyan = new(0.38f, 0.78f, 0.86f);
        public static readonly Color NeonPurple = new(0.52f, 0.42f, 0.72f);
        public static readonly Color NeonGreen = new(0.42f, 0.78f, 0.52f);
        public static readonly Color NeonYellow = new(0.92f, 0.78f, 0.38f);

        public static readonly Color Danger = new(0.82f, 0.32f, 0.36f);
        public static readonly Color PoliceBlue = new(0.28f, 0.42f, 0.78f);
        public static readonly Color DirtyCash = new(0.55f, 0.72f, 0.42f);
        public static readonly Color CleanCash = new(0.72f, 0.82f, 0.88f);
        public static readonly Color Diamond = new(0.55f, 0.78f, 0.9f);

        // World / UI surfaces (dusk asphalt, not loud purple voids)
        public static readonly Color Background = new(0.09f, 0.1f, 0.14f);
        public static readonly Color Ambient = new(0.42f, 0.4f, 0.48f);
        public static readonly Color Fog = new(0.22f, 0.24f, 0.32f);
        public static readonly Color Ground = new(0.22f, 0.22f, 0.26f);
        public static readonly Color Building = new(0.28f, 0.26f, 0.3f);
        public static readonly Color BuildingAlt = new(0.24f, 0.27f, 0.32f);
        public static readonly Color Metal = new(0.32f, 0.33f, 0.36f);
        public static readonly Color Skin = new(0.78f, 0.62f, 0.52f);
        public static readonly Color Outline = new(0.06f, 0.05f, 0.08f);

        public static readonly Color PanelBg = new(0.08f, 0.09f, 0.12f, 0.62f);
        public static readonly Color PanelBorder = new(0.35f, 0.38f, 0.45f, 0.55f);
        public static readonly Color ButtonBg = new(0.12f, 0.13f, 0.17f, 0.82f);
        public static readonly Color TextDim = new(0.62f, 0.64f, 0.7f);
        public static readonly Color RimAccent = new(0.55f, 0.7f, 0.78f);

        public static Color ForRarity(Rarity rarity) => rarity switch
        {
            Rarity.Common => new Color(0.62f, 0.64f, 0.68f),
            Rarity.Uncommon => new Color(0.38f, 0.72f, 0.48f),
            Rarity.Rare => new Color(0.38f, 0.62f, 0.85f),
            Rarity.Epic => new Color(0.62f, 0.42f, 0.78f),
            Rarity.Legendary => new Color(0.88f, 0.7f, 0.35f),
            Rarity.Boss => new Color(0.85f, 0.35f, 0.4f),
            _ => Colors.White
        };

        public static Color ForStatus(StatusType status) => status switch
        {
            StatusType.Bleed => new Color(0.78f, 0.22f, 0.28f),
            StatusType.Poison => new Color(0.52f, 0.72f, 0.28f),
            StatusType.Confused => NeonPink,
            StatusType.Stunned => NeonYellow,
            StatusType.Banished => new Color(0.55f, 0.55f, 0.62f),
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

        /// <summary>Soft shade tint mixed into toon materials (desaturated, slight cool bias).</summary>
        public static Color ShadeFrom(Color albedo) =>
            albedo.Darkened(0.45f).Lerp(new Color(0.22f, 0.22f, 0.28f), 0.4f);
    }
}
