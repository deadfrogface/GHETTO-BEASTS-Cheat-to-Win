using GhettoBeasts.Data;

namespace GhettoBeasts.Combat
{
    /// <summary>Light classification for support / crowd-control skill presentation.</summary>
    public enum SkillCinematicKind
    {
        Strike,
        Poison,
        Charm,
        Stun,
        Support,
        MultiHit,
        Special
    }

    public static class SkillCinematics
    {
        public static SkillCinematicKind Classify(AbilityId ability) => ability switch
        {
            AbilityId.ToxicSpit or AbilityId.AcidSpray or AbilityId.DirtyNeedle => SkillCinematicKind.Poison,
            AbilityId.Seduction or AbilityId.MadLaugh => SkillCinematicKind.Charm,
            AbilityId.JawLock or AbilityId.DeathRoll or AbilityId.ChestPound or AbilityId.HouseBan => SkillCinematicKind.Stun,
            AbilityId.WrenchTuning or AbilityId.BackalleySurgery => SkillCinematicKind.Support,
            AbilityId.Frenzy or AbilityId.CallBodyguards => SkillCinematicKind.MultiHit,
            AbilityId.LaserEyes or AbilityId.DeathDance => SkillCinematicKind.Special,
            _ => SkillCinematicKind.Strike
        };

        public static bool IsSupport(AbilityId ability) => Classify(ability) == SkillCinematicKind.Support;

        public static bool IsCrowdControl(AbilityId ability)
        {
            var kind = Classify(ability);
            return kind is SkillCinematicKind.Poison or SkillCinematicKind.Charm or SkillCinematicKind.Stun;
        }
    }
}
