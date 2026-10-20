using GhettoBeasts.Core;

namespace GhettoBeasts.Combat
{
    public enum InstabilityOutcome
    {
        None,
        Confusion,
        CellFailure,
        Rage,
        WildMutation,
        Collapse
    }

    public static class InstabilityResolver
    {
        public const int ConfusionThreshold = 50;
        public const int CriticalThreshold = 80;
        public const double ConfusionChance = 15;
        public const double CriticalChance = 35;

        /// <summary>
        /// &gt;50%: 15% confusion. &gt;80%: 35% cell failure or rage. 100%: spontaneous mutation or collapse.
        /// </summary>
        public static InstabilityOutcome Roll(int instability, IRandom random, bool alreadyMutated = false)
        {
            if (instability >= Combatant.MaxInstability)
            {
                if (alreadyMutated) return random.Chance(CriticalChance) ? InstabilityOutcome.Rage : InstabilityOutcome.None;
                return random.Chance(50) ? InstabilityOutcome.WildMutation : InstabilityOutcome.Collapse;
            }

            if (instability > CriticalThreshold)
            {
                if (!random.Chance(CriticalChance)) return InstabilityOutcome.None;
                return random.Chance(50) ? InstabilityOutcome.CellFailure : InstabilityOutcome.Rage;
            }

            if (instability > ConfusionThreshold)
                return random.Chance(ConfusionChance) ? InstabilityOutcome.Confusion : InstabilityOutcome.None;

            return InstabilityOutcome.None;
        }
    }
}
