using Xunit;

namespace GhettoBeasts.Core.Tests;

public class StreetProgressionTests
{
    [Fact]
    public void Defaults_unlock_core_buildings()
    {
        var save = SaveData.CreateDefault();
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Arena));
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Crispr));
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Wash));
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Darknet));
        Assert.False(StreetProgression.IsUnlocked(save, StreetProgression.Dope));
        Assert.False(StreetProgression.IsUnlocked(save, StreetProgression.BackAlley));
    }

    [Fact]
    public void Wins_unlock_dope_weap_and_blocks()
    {
        var save = SaveData.CreateDefault();
        StreetProgression.RegisterWin(save);
        StreetProgression.RegisterWin(save);
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Dope));
        Assert.False(StreetProgression.IsUnlocked(save, StreetProgression.Weap));

        StreetProgression.RegisterWin(save);
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.Weap));

        save.StreetWins = 5;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.BackAlley));

        save.StreetWins = 8;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.True(StreetProgression.IsUnlocked(save, StreetProgression.DockBlock));
        Assert.Contains("q_dock", save.CompletedQuestIds);
    }

    [Fact]
    public void Double_battle_stub_lands_partner_assist()
    {
        var economy = new Economy(100, 0, 0);
        var player = Fixtures.Player("kupferstecher_pitbull");
        var enemy = Fixtures.Enemy("kanalratte");
        int enemyHp = enemy.Hp;
        var events = new List<Combat.BattleEvent>();
        var engine = new Combat.BattleEngine(player, enemy, economy, new ScriptedRandom(0.99),
            new Combat.CombatModifiers { DoubleBattleStub = true, PartnerAssistScale = 0.5 });
        engine.EventRaised += events.Add;
        engine.Start();
        Assert.True(engine.DoubleBattleStub);
        Assert.True(engine.IsPlayersTurn);
        Assert.True(engine.PlayerAttack());
        Assert.True(enemy.Hp < enemyHp);
        Assert.Contains(events, e => e.Message.Contains("2v2-Partner", StringComparison.Ordinal));
    }

    [Fact]
    public void Save_roundtrip_keeps_progression()
    {
        var save = SaveData.CreateDefault();
        save.StreetWins = 5;
        StreetProgression.SyncUnlocksFromWins(save);
        save.PreferDoubleBattle = true;
        string json = SaveSerializer.Serialize(save);
        var loaded = SaveSerializer.Deserialize(json);
        Assert.Equal(5, loaded.StreetWins);
        Assert.True(StreetProgression.IsUnlocked(loaded, StreetProgression.BackAlley));
        Assert.True(loaded.PreferDoubleBattle);
        Assert.Equal(SaveData.CurrentVersion, loaded.Version);
    }
}
