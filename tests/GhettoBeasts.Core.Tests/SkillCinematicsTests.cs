using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class SkillCinematicsTests
{
    [Theory]
    [InlineData(AbilityId.WrenchTuning, SkillCinematicKind.Support)]
    [InlineData(AbilityId.BackalleySurgery, SkillCinematicKind.Support)]
    [InlineData(AbilityId.Seduction, SkillCinematicKind.Charm)]
    [InlineData(AbilityId.MadLaugh, SkillCinematicKind.Charm)]
    [InlineData(AbilityId.ToxicSpit, SkillCinematicKind.Poison)]
    [InlineData(AbilityId.JawLock, SkillCinematicKind.Stun)]
    [InlineData(AbilityId.HouseBan, SkillCinematicKind.Stun)]
    [InlineData(AbilityId.Frenzy, SkillCinematicKind.MultiHit)]
    [InlineData(AbilityId.LaserEyes, SkillCinematicKind.Special)]
    public void Classify_maps_catalog_abilities(AbilityId ability, SkillCinematicKind expected)
    {
        Assert.Equal(expected, SkillCinematics.Classify(ability));
    }

    [Fact]
    public void Support_and_crowd_control_helpers()
    {
        Assert.True(SkillCinematics.IsSupport(AbilityId.WrenchTuning));
        Assert.True(SkillCinematics.IsCrowdControl(AbilityId.Seduction));
        Assert.True(SkillCinematics.IsCrowdControl(AbilityId.ToxicSpit));
        Assert.False(SkillCinematics.IsCrowdControl(AbilityId.LaserEyes));
    }
}
