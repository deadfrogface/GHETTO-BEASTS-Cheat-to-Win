using GhettoBeasts.Data;
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

    [Theory]
    [InlineData(0, StreetTier.Rookie)]
    [InlineData(1, StreetTier.Rookie)]
    [InlineData(2, StreetTier.DopeRun)]
    [InlineData(3, StreetTier.Armed)]
    [InlineData(5, StreetTier.BackAlley)]
    [InlineData(8, StreetTier.Dock)]
    [InlineData(99, StreetTier.Dock)]
    public void Street_tier_follows_quest_thresholds(int wins, StreetTier expected)
    {
        Assert.Equal(expected, StreetProgression.GetStreetTier(wins));
    }

    [Fact]
    public void Wild_pool_escalates_rarity_with_tier()
    {
        var rookie = SaveData.CreateDefault();
        var dock = SaveData.CreateDefault();
        dock.StreetWins = 8;
        StreetProgression.SyncUnlocksFromWins(dock);

        var rookiePool = StreetProgression.BuildWildPool(rookie);
        var dockPool = StreetProgression.BuildWildPool(dock);
        Assert.NotEmpty(rookiePool);
        Assert.NotEmpty(dockPool);

        Assert.All(rookiePool, id =>
        {
            var card = ContentCatalog.AllCards.First(c => c.Id == id);
            Assert.True(card.Rarity <= Rarity.Uncommon);
            Assert.Equal(CardKind.Beast, card.Kind);
        });

        Assert.Contains(dockPool, id =>
        {
            var card = ContentCatalog.AllCards.First(c => c.Id == id);
            return card.Rarity >= Rarity.Rare;
        });
        Assert.DoesNotContain(dockPool, id =>
        {
            var card = ContentCatalog.AllCards.First(c => c.Id == id);
            return card.Rarity < Rarity.Rare;
        });
    }

    [Fact]
    public void Milestone_boss_unlocks_with_street_wins()
    {
        var save = SaveData.CreateDefault();
        Assert.Null(StreetProgression.MilestoneBossId(save));

        save.StreetWins = 2;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.Equal("ede", StreetProgression.MilestoneBossId(save));

        save.StreetWins = 3;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.Equal("dr_mutation", StreetProgression.MilestoneBossId(save));

        save.StreetWins = 5;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.Equal("lilith", StreetProgression.MilestoneBossId(save));

        save.StreetWins = 8;
        StreetProgression.SyncUnlocksFromWins(save);
        Assert.Equal("el_capo", StreetProgression.MilestoneBossId(save));
    }

    [Fact]
    public void Status_line_includes_tier_label()
    {
        var save = SaveData.CreateDefault();
        save.StreetWins = 5;
        StreetProgression.SyncUnlocksFromWins(save);
        string line = StreetProgression.StatusLine(save);
        Assert.Contains("Tier 3", line);
        Assert.Contains("Back Alley", line);
    }

    [Fact]
    public void Campaign_tag_marks_street_v2_build()
    {
        Assert.Contains("STREET-v2", StreetProgression.CampaignTag);
    }
}
