namespace GhettoBeasts.Combat
{
    /// <summary>Burner-phone contact buffs applied when constructing a battle.</summary>
    public sealed class CombatModifiers
    {
        public static CombatModifiers None { get; } = new();

        /// <summary>Multiplier on SWAT suspicion gains (lawyer = 0.85).</summary>
        public double SuspicionGainScale { get; init; } = 1.0;

        /// <summary>When true, referee bribe spends clean flatrate instead of dirty cash.</summary>
        public bool RefFlatrate { get; init; }

        public int RefFlatrateCleanCost { get; init; } = 40;
    }
}
