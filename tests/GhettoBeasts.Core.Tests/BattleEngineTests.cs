using GhettoBeasts.Combat;
using Xunit;

namespace GhettoBeasts.Core.Tests;

public class BattleEngineTests
{
    [Fact]
    public void Faster_player_starts_in_player_turn()
    {
        var (engine, events) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"));

        Assert.Equal(BattleState.PlayerTurn, engine.State);
        Assert.Equal(2, events.Count(e => e.Type == BattleEventType.Summon));
    }

    [Fact]
    public void Slower_player_lets_enemy_act_first()
    {
        var (engine, events) = Fixtures.Battle(Fixtures.Player("kanal_alligator"), Fixtures.Enemy("kanalratte"));

        Assert.Equal(BattleState.PlayerTurn, engine.State);
        Assert.Contains(events, e => e.Type == BattleEventType.Damage && e.Target == Side.Player);
    }

    [Fact]
    public void Attack_runs_full_round_and_returns_to_player()
    {
        var (engine, _) = Fixtures.Battle(Fixtures.Player("tiefgaragen_dobermann"), Fixtures.Enemy("kupferstecher_pitbull"));

        Assert.True(engine.PlayerAttack());
        Assert.Equal(BattleState.PlayerTurn, engine.State);
        Assert.Equal(2, engine.Round);
        Assert.True(engine.Enemy.Hp < engine.Enemy.Stats.MaxHp);
        Assert.True(engine.Player.Hp < engine.Player.Stats.MaxHp);
    }

    [Fact]
    public void Skill_goes_on_cooldown()
    {
        var (engine, _) = Fixtures.Battle(Fixtures.Player("cyber_pit"), Fixtures.Enemy("el_capo"));

        Assert.True(engine.PlayerSkill());
        Assert.False(engine.CanUseSkill);
        Assert.False(engine.PlayerSkill());
        for (int i = 0; i < BattleEngine.SkillCooldownTurns; i++) engine.PlayerAttack();
        Assert.True(engine.CanUseSkill);
    }

    [Fact]
    public void Jaw_lock_skips_enemy_action()
    {
        var (engine, events) = Fixtures.Battle(Fixtures.Player("kupferstecher_pitbull"), Fixtures.Enemy("kanal_alligator"));
        int hpBefore = engine.Player.Hp;
        events.Clear();

        engine.PlayerSkill();

        Assert.Equal(hpBefore, engine.Player.Hp);
        Assert.Contains(events, e => e.Message.Contains("betäubt"));
    }

    [Fact]
    public void Only_one_cheat_per_turn_and_doping_costs_dirty_cash()
    {
        var economy = new Economy(500);
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"), economy);

        Assert.True(engine.PlayerCheat(CheatType.Doping));
        Assert.Equal(BattleState.PlayerTurn, engine.State);
        Assert.True(engine.Player.Has(StatusType.Doped));
        Assert.Equal(500 - CheatManager.DopingCost, economy.DirtyCash);
        Assert.Equal(CheatManager.DopingSuspicion, engine.Suspicion.Value);
        Assert.False(engine.PlayerCheat(CheatType.Brick));
        Assert.True(engine.PlayerAttack());
        Assert.True(engine.PlayerCheat(CheatType.Brick));
    }

    [Fact]
    public void Brick_hit_knocks_out_trainer_and_wins()
    {
        var random = new ScriptedRandom();
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"), random: random);
        random.Enqueue(0.05);

        Assert.True(engine.PlayerCheat(CheatType.Brick));
        Assert.Equal(BattleState.Victory, engine.State);
        Assert.Equal(BattleOutcome.Victory, engine.Outcome);
    }

    [Fact]
    public void Suspicion_at_100_triggers_raid_and_confiscates_dirty_cash()
    {
        var economy = new Economy(1000, 50, 0);
        var (engine, events) = Fixtures.Battle(Fixtures.Player("kanal_alligator"), Fixtures.Enemy("silberruecken_gorilla"), economy);

        for (int i = 0; i < 3 && !engine.IsOver; i++)
        {
            engine.PlayerCheat(CheatType.Brick);
            if (!engine.IsOver) engine.PlayerAttack();
        }

        Assert.Equal(BattleState.Raid, engine.State);
        Assert.Equal(BattleOutcome.Raid, engine.Outcome);
        Assert.Equal(0, economy.DirtyCash);
        Assert.Equal(50, economy.CleanCash);
        Assert.Contains(events, e => e.Type == BattleEventType.Raid);
    }

    [Fact]
    public void Bribe_lowers_suspicion()
    {
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"), new Economy(1000));
        engine.PlayerCheat(CheatType.Brick);
        engine.PlayerAttack();

        Assert.True(engine.PlayerCheat(CheatType.Bribe));
        Assert.Equal(0, engine.Suspicion.Value);
    }

    [Fact]
    public void Cheats_rejected_without_money()
    {
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"), new Economy(0));
        string? error = null;
        engine.InvalidAction += msg => error = msg;

        Assert.False(engine.PlayerCheat(CheatType.Bribe));
        Assert.NotNull(error);
        Assert.Equal(0, engine.Suspicion.Value);
    }

    [Fact]
    public void Camouflage_dodges_attacks_in_first_round()
    {
        var (engine, events) = Fixtures.Battle(
            Fixtures.Player("kanalratte"), Fixtures.Enemy("kupferstecher_pitbull", "chamaeleon_tarnhaut"));
        events.Clear();

        engine.PlayerAttack();

        Assert.Contains(events, e => e.Type == BattleEventType.Miss && e.Target == Side.Enemy);
    }

    [Fact]
    public void House_ban_removes_buffs_and_banishes()
    {
        var (engine, events) = Fixtures.Battle(Fixtures.Player("ede"), Fixtures.Enemy("kanal_alligator"));
        int hpBefore = engine.Player.Hp;

        engine.PlayerSkill();

        Assert.Equal(hpBefore, engine.Player.Hp);
        Assert.Contains(events, e => e.Message.Contains("Hand fest"));
    }

    [Fact]
    public void Shock_skin_counters_attacker()
    {
        var (engine, events) = Fixtures.Battle(
            Fixtures.Player("tiefgaragen_dobermann"), Fixtures.Enemy("kupferstecher_pitbull", "aal_strom_haut"));
        events.Clear();

        engine.PlayerAttack();

        Assert.Contains(events, e => e.Message.StartsWith("Aal-Strom") && e.Target == Side.Player);
    }

    [Fact]
    public void Victory_pays_rarity_reward()
    {
        var economy = new Economy(0);
        var random = new ScriptedRandom();
        var (engine, _) = Fixtures.Battle(Fixtures.Player("cyber_pit"), Fixtures.Enemy("kanalratte"), economy, random);

        while (!engine.IsOver) engine.PlayerAttack();

        Assert.Equal(BattleOutcome.Victory, engine.Outcome);
        Assert.Equal(50, economy.DirtyCash);
    }

    [Fact]
    public void Max_instability_can_collapse_beast()
    {
        var random = new ScriptedRandom();
        var player = Fixtures.Player("crispr_01", "piranha_gebiss", "klingen_pranken", "aal_strom_haut");
        Assert.Equal(90, player.Instability);
        var (engine, _) = Fixtures.Battle(player, Fixtures.Enemy("kanal_alligator"), new Economy(1000), random);

        engine.PlayerCheat(CheatType.Doping);
        Assert.Equal(98, player.Instability);
        engine.PlayerAttack();
        engine.PlayerCheat(CheatType.Doping);
        Assert.Equal(100, player.Instability);
        random.Enqueue(0.9);
        engine.PlayerAttack();

        Assert.Equal(BattleOutcome.Defeat, engine.Outcome);
    }

    [Fact]
    public void Invalid_actions_after_battle_are_reported()
    {
        var random = new ScriptedRandom();
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"), random: random);
        random.Enqueue(0.0);
        engine.PlayerCheat(CheatType.Brick);
        var errors = new List<string>();
        engine.InvalidAction += errors.Add;

        Assert.False(engine.PlayerAttack());
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void State_graph_can_be_exported()
    {
        var (engine, _) = Fixtures.Battle(Fixtures.Player("kanalratte"), Fixtures.Enemy("kanal_alligator"));
        Assert.Contains("CheatPhase", engine.ToDotGraph());
    }
}
