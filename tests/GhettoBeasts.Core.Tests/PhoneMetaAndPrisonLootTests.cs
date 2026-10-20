using GhettoBeasts.Combat;
using GhettoBeasts.Data;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class PhoneMetaAndPrisonLootTests
{
    [Fact]
    public void Lawyer_unlock_scales_suspicion_gain()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(0, 500, 0);
        Assert.True(ContactsService.TryUnlockLawyer(save, economy, out _));
        Assert.True(save.LawyerUnlocked);
        Assert.Equal(ContactsService.LawyerSuspicionScale, save.SuspicionGainScale, 3);

        var mods = new CombatModifiers { SuspicionGainScale = save.SuspicionGainScale };
        var suspicion = new SuspicionMeter();
        var cheats = new CheatManager(economy, suspicion, new ScriptedRandom(0.99), mods);
        var player = Fixtures.Player("kupferstecher_pitbull");
        var enemy = Fixtures.Enemy("kanalratte");
        cheats.Execute(CheatType.Brick, player, enemy, _ => { });
        Assert.Equal((int)Math.Round(CheatManager.BrickSuspicion * ContactsService.LawyerSuspicionScale), suspicion.Value);
    }

    [Fact]
    public void Ref_flatrate_spends_clean_not_dirty()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(500, 400, 0);
        Assert.True(ContactsService.TryUnlockRef(save, economy, out _));
        var mods = new CombatModifiers
        {
            RefFlatrate = true,
            RefFlatrateCleanCost = ContactsService.RefFlatrateClean
        };
        var suspicion = new SuspicionMeter();
        suspicion.Add(50);
        var cheats = new CheatManager(economy, suspicion, new ScriptedRandom(0.01), mods);
        int dirtyBefore = economy.DirtyCash;
        int cleanBefore = economy.CleanCash;
        var player = Fixtures.Player("kupferstecher_pitbull");
        var enemy = Fixtures.Enemy("kanalratte");
        Assert.True(cheats.CanAfford(CheatType.Bribe));
        cheats.Execute(CheatType.Bribe, player, enemy, _ => { });
        Assert.Equal(dirtyBefore, economy.DirtyCash);
        Assert.Equal(cleanBefore - ContactsService.RefFlatrateClean, economy.CleanCash);
    }

    [Fact]
    public void Laundry_timer_claim_converts_dirty()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(200, 0, 0);
        Assert.True(LaundryService.TryStart(save, economy, 100, out _, durationSeconds: 1));
        Assert.Equal(100, economy.DirtyCash);
        Assert.True(save.Laundry.Active);
        save.Laundry.ReadyUnix = LaundryJob.NowUnix() - 1;
        Assert.True(LaundryService.TryClaim(save, economy, out _));
        Assert.Equal(80, economy.CleanCash);
        Assert.False(save.Laundry.Active);
    }

    [Fact]
    public void Prison_fail_confiscates_half_dirty()
    {
        var economy = new Economy(200, 0, 0);
        var engine = new PrisonEscapeEngine(economy, new ScriptedRandom(0.5), seed: 3);
        for (int i = 0; i < 40 && !engine.IsOver; i++)
            engine.TryRest();

        Assert.Equal(PrisonOutcome.Caught, engine.Outcome);
        Assert.Equal(100, economy.DirtyCash);
        Assert.Equal(100, engine.DirtyLostOnFail);
    }

    [Fact]
    public void Gacha_can_roll_gen_corrupted_rarity()
    {
        var save = SaveData.CreateDefault();
        var economy = new Economy(0, 0, 5);
        // Weight table ends with GenCorrupted (~1.2 / ~98.2) — high NextDouble hits it.
        var rng = new ScriptedRandom(0.995, 0.9);
        var result = DarknetGacha.OpenPack(save, economy, rng);
        Assert.True(result.Success);
        Assert.NotNull(result.Pull);
        Assert.Equal(Rarity.GenCorrupted, result.Pull!.Rarity);
    }

    [Fact]
    public void Confiscate_half_dirty_rounds_up()
    {
        var economy = new Economy(5, 0, 0);
        Assert.Equal(3, economy.ConfiscateHalfDirtyCash());
        Assert.Equal(2, economy.DirtyCash);
    }
}
