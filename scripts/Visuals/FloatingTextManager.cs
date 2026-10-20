using GhettoBeasts.Combat;
using Godot;

namespace GhettoBeasts.Visuals
{
    public enum CombatTextKind
    {
        Normal,
        Crit,
        Poison,
        Charm,
        SelfDamage
    }

    /// <summary>Typed floating combat text with fixed juice colors.</summary>
    public static class FloatingTextManager
    {
        public static readonly Color Normal = Colors.White;
        public static readonly Color Crit = Palette.NeonYellow;
        public static readonly Color Poison = new(0.52f, 0.88f, 0.28f);
        public static readonly Color Charm = Palette.NeonPink;
        public static readonly Color SelfDamage = new(1f, 0.45f, 0.35f);

        public static Color ColorFor(CombatTextKind kind) => kind switch
        {
            CombatTextKind.Crit => Crit,
            CombatTextKind.Poison => Poison,
            CombatTextKind.Charm => Charm,
            CombatTextKind.SelfDamage => SelfDamage,
            _ => Normal
        };

        public static int FontSizeFor(CombatTextKind kind) => kind switch
        {
            CombatTextKind.Crit => 96,
            CombatTextKind.SelfDamage => 78,
            CombatTextKind.Poison or CombatTextKind.Charm => 58,
            _ => 68
        };

        public static void Show(Node parent, Vector3 position, string text, CombatTextKind kind)
        {
            Vfx.FloatingCombatText(parent, position, text, ColorFor(kind), FontSizeFor(kind));
        }

        public static void Damage(Node parent, Vector3 position, int amount, bool critical, bool selfHit = false)
        {
            if (selfHit)
                Show(parent, position, $"-{amount}", CombatTextKind.SelfDamage);
            else if (critical)
                Show(parent, position, $"-{amount}!", CombatTextKind.Crit);
            else
                Show(parent, position, $"-{amount}", CombatTextKind.Normal);
        }

        public static void Status(Node parent, Vector3 position, StatusType status)
        {
            var kind = status switch
            {
                StatusType.Poison => CombatTextKind.Poison,
                StatusType.Confused => CombatTextKind.Charm,
                _ => CombatTextKind.Normal
            };
            Color color = kind == CombatTextKind.Normal ? Palette.ForStatus(status) : ColorFor(kind);
            Vfx.FloatingCombatText(parent, position, Palette.StatusName(status).ToUpperInvariant(), color, FontSizeFor(kind));
        }
    }
}
