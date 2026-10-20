using System;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using Stateless;

namespace GhettoBeasts.Combat
{
    public enum BattleState
    {
        Idle,
        Summon,
        PlayerTurn,
        CheatPhase,
        Resolving,
        EnemyTurn,
        Victory,
        Defeat,
        Raid
    }

    public enum BattleOutcome
    {
        Victory,
        Defeat,
        Raid
    }

    public enum ActionKind
    {
        Attack,
        Skill
    }

    /// <summary>
    /// Turn-based 1v1 battle. All rules are resolved synchronously; every visible step is published
    /// through <see cref="EventRaised"/> so the presentation layer can replay it with animations.
    /// </summary>
    public sealed partial class BattleEngine
    {
        public const int SkillCooldownTurns = 3;
        public const int BasicAttackPower = 100;

        private enum Trigger
        {
            Start,
            SummonComplete,
            Attack,
            Skill,
            Cheat,
            CheatResolved,
            ActionResolved,
            EnemyActed,
            Win,
            Lose,
            SuspicionMaxed
        }

        private readonly StateMachine<BattleState, Trigger> machine;
        private readonly Economy economy;
        private readonly IRandom random;
        private readonly CheatManager cheats;
        private readonly CombatModifiers modifiers;

        private ActionKind pendingAction;
        private CheatType pendingCheat;
        private bool cheatUsedThisTurn;
        private int actionsTaken;

        public BattleEngine(Combatant player, Combatant enemy, Economy economy, IRandom? random = null,
            CombatModifiers? modifiers = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            if (player.Side != Side.Player || enemy.Side != Side.Enemy)
                throw new ArgumentException("Combatant-Seiten sind vertauscht.");

            this.economy = economy ?? throw new ArgumentNullException(nameof(economy));
            this.random = random ?? new SystemRandom();
            this.modifiers = modifiers ?? CombatModifiers.None;
            Suspicion = new SuspicionMeter();
            cheats = new CheatManager(economy, Suspicion, this.random, this.modifiers);

            machine = new StateMachine<BattleState, Trigger>(BattleState.Idle, FiringMode.Queued);
            ConfigureStateMachine();
        }

        public event Action<BattleEvent>? EventRaised;
        public event Action<BattleState>? StateChanged;
        public event Action<string>? InvalidAction;

        public Combatant Player { get; }
        public Combatant Enemy { get; }
        public SuspicionMeter Suspicion { get; }
        public BattleState State => machine.State;
        public BattleOutcome? Outcome { get; private set; }
        public int Round => actionsTaken / 2 + 1;
        public bool IsOver => Outcome.HasValue;
        public bool IsPlayersTurn => machine.IsInState(BattleState.PlayerTurn) && !IsOver;
        public bool CanAttack => IsPlayersTurn && machine.CanFire(Trigger.Attack);
        public bool CanUseSkill => IsPlayersTurn && machine.CanFire(Trigger.Skill);
        public bool CheatUsedThisTurn => cheatUsedThisTurn;
        public bool DoubleBattleStub => modifiers.DoubleBattleStub;

        public bool CanCheat(CheatType cheat) => IsPlayersTurn && machine.CanFire(Trigger.Cheat) && cheats.CanAfford(cheat);

        /// <summary>Exports the state graph in DOT format (visualize e.g. with Graphviz).</summary>
        public string ToDotGraph() => Stateless.Graph.UmlDotGraph.Format(machine.GetInfo());

        public void Start() => TryFire(Trigger.Start, "Kampf läuft bereits.");

        public bool PlayerAttack()
        {
            if (!CanAttack) return Reject("Angriff ist gerade nicht möglich.");
            pendingAction = ActionKind.Attack;
            return TryFire(Trigger.Attack, "Angriff ist gerade nicht möglich.");
        }

        public bool PlayerSkill()
        {
            if (!CanUseSkill)
                return Reject(Player.SkillReady ? "Skill ist gerade nicht möglich." : $"{Player.Definition.AbilityName} lädt noch ({Player.SkillCooldown} Züge).");
            pendingAction = ActionKind.Skill;
            return TryFire(Trigger.Skill, "Skill ist gerade nicht möglich.");
        }

        public bool PlayerCheat(CheatType cheat)
        {
            if (!IsPlayersTurn) return Reject("Sabotage nur im eigenen Zug.");
            if (cheatUsedThisTurn) return Reject("Nur eine Sabotage pro Zug.");
            if (!cheats.CanAfford(cheat)) return Reject($"Zu wenig Schmutzgeld für {CheatManager.DisplayName(cheat)} ({CheatManager.CostOf(cheat)} $).");
            pendingCheat = cheat;
            return TryFire(Trigger.Cheat, "Sabotage ist gerade nicht möglich.");
        }

        private void ConfigureStateMachine()
        {
            machine.OnTransitioned(t => StateChanged?.Invoke(t.Destination));
            machine.OnUnhandledTrigger((state, trigger) =>
                InvalidAction?.Invoke($"Ungültiger Übergang: {trigger} im Zustand {state}."));

            machine.Configure(BattleState.Idle)
                .Permit(Trigger.Start, BattleState.Summon);

            machine.Configure(BattleState.Summon)
                .OnEntry(OnSummon)
                .PermitDynamic(Trigger.SummonComplete, () => PlayerActsFirst() ? BattleState.PlayerTurn : BattleState.EnemyTurn);

            machine.Configure(BattleState.PlayerTurn)
                .OnEntry(() =>
                {
                    cheatUsedThisTurn = false;
                    Emit(BattleEventType.Log, $"— Runde {Round}: Dein Zug —");
                })
                .Permit(Trigger.Attack, BattleState.Resolving)
                .PermitIf(Trigger.Skill, BattleState.Resolving, () => Player.SkillReady)
                .PermitIf(Trigger.Cheat, BattleState.CheatPhase, () => !cheatUsedThisTurn)
                .Permit(Trigger.Win, BattleState.Victory)
                .Permit(Trigger.Lose, BattleState.Defeat)
                .Permit(Trigger.SuspicionMaxed, BattleState.Raid);

            machine.Configure(BattleState.CheatPhase)
                .SubstateOf(BattleState.PlayerTurn)
                .OnEntry(OnCheatPhase)
                .Permit(Trigger.CheatResolved, BattleState.PlayerTurn);

            machine.Configure(BattleState.Resolving)
                .OnEntry(OnResolvePlayerAction)
                .Permit(Trigger.ActionResolved, BattleState.EnemyTurn)
                .Permit(Trigger.Win, BattleState.Victory)
                .Permit(Trigger.Lose, BattleState.Defeat);

            machine.Configure(BattleState.EnemyTurn)
                .OnEntry(OnEnemyTurn)
                .Permit(Trigger.EnemyActed, BattleState.PlayerTurn)
                .Permit(Trigger.Win, BattleState.Victory)
                .Permit(Trigger.Lose, BattleState.Defeat);

            machine.Configure(BattleState.Victory).OnEntry(OnVictory);
            machine.Configure(BattleState.Defeat).OnEntry(OnDefeat);
            machine.Configure(BattleState.Raid).OnEntry(OnRaid);
        }

        private bool PlayerActsFirst() => Player.Stats.Speed >= Enemy.Stats.Speed;

        private void OnSummon()
        {
            Emit(new BattleEvent(BattleEventType.Summon, $"Du spielst {Player.Name} aus!", Side.Player, ValueAfter: Player.Hp));
            Emit(new BattleEvent(BattleEventType.Summon, $"Der Gegner beschwört {Enemy.Name}!", Side.Enemy, ValueAfter: Enemy.Hp));
            foreach (var combatant in new[] { Player, Enemy })
            {
                if (combatant.HasEffect(MutationEffect.CamouflageFirstRound))
                    Emit(BattleEventType.Log, $"{combatant.Name} verschwimmt mit dem Asphalt – unsichtbar in Runde 1.", combatant.Side);
            }
            Emit(BattleEventType.Log, PlayerActsFirst() ? "Du hast die Initiative." : $"{Enemy.Name} ist schneller!");
            machine.Fire(Trigger.SummonComplete);
        }

        private void OnCheatPhase()
        {
            cheatUsedThisTurn = true;
            var result = cheats.Execute(pendingCheat, Player, Enemy, Emit);

            if (Suspicion.IsMaxed)
                machine.Fire(Trigger.SuspicionMaxed);
            else if (result.TrainerKnockedOut)
                machine.Fire(Trigger.Win);
            else
                machine.Fire(Trigger.CheatResolved);
        }

        private void OnResolvePlayerAction()
        {
            ExecuteAction(Player, Enemy, pendingAction);
            ApplyPartnerAssist(Player, Enemy);
            FireAfterAction(Trigger.ActionResolved);
        }

        private void OnEnemyTurn()
        {
            Emit(BattleEventType.Log, $"— Runde {Round}: {Enemy.Name} ist am Zug —");
            ExecuteAction(Enemy, Player, ChooseEnemyAction());
            ApplyPartnerAssist(Enemy, Player);
            FireAfterAction(Trigger.EnemyActed);
        }

        /// <summary>Stub 2v2 — partner lands a weaker follow-up hit when the mode is enabled.</summary>
        private void ApplyPartnerAssist(Combatant lead, Combatant target)
        {
            if (!modifiers.DoubleBattleStub || lead.IsDown || target.IsDown) return;
            int power = Math.Max(20, (int)Math.Round(BasicAttackPower * modifiers.PartnerAssistScale));
            Emit(BattleEventType.Log, $"2v2-Partner assistiert ({power}% Power)!", lead.Side);
            // Hit lives on the Actions partial; call via a thin wrapper.
            PartnerHit(lead, target, power);
        }

        private void FireAfterAction(Trigger continueTrigger)
        {
            if (Enemy.IsDown) machine.Fire(Trigger.Win);
            else if (Player.IsDown) machine.Fire(Trigger.Lose);
            else machine.Fire(continueTrigger);
        }

        private ActionKind ChooseEnemyAction()
        {
            if (!Enemy.SkillReady) return ActionKind.Attack;
            bool isHeal = Enemy.Definition.Ability is AbilityId.WrenchTuning or AbilityId.BackalleySurgery;
            if (isHeal && Enemy.Hp > Enemy.Stats.MaxHp * 0.6) return ActionKind.Attack;
            return random.Chance(55) ? ActionKind.Skill : ActionKind.Attack;
        }

        private void OnVictory()
        {
            Outcome = BattleOutcome.Victory;
            int reward = ContentCatalog.DirtyCashReward(Enemy.Definition.Rarity);
            economy.Add(Currency.DirtyCash, reward);
            Emit(new BattleEvent(BattleEventType.Cash, $"+{reward} $ Schmutzgeld", Side.Player, reward, economy.DirtyCash));
            if (Enemy.Definition.Rarity is Rarity.Legendary or Rarity.Boss)
            {
                economy.Add(Currency.Diamonds, 1);
                Emit(BattleEventType.Log, "Seltener Drop: +1 Dirty Diamond!");
            }
            Emit(new BattleEvent(BattleEventType.Result, $"SIEG! {Enemy.Name} liegt am Boden.", Side.Player));
        }

        private void OnDefeat()
        {
            Outcome = BattleOutcome.Defeat;
            Emit(new BattleEvent(BattleEventType.Result, $"NIEDERLAGE. {Player.Name} ist erledigt.", Side.Enemy));
        }

        private void OnRaid()
        {
            Outcome = BattleOutcome.Raid;
            Emit(new BattleEvent(BattleEventType.Raid, "SWAT-RAZZIA! Die Arena wird gestürmt!"));
            int lost = economy.ConfiscateDirtyCash();
            Emit(new BattleEvent(BattleEventType.Cash, $"Beschlagnahmt: {lost} $ Schmutzgeld", Side.Player, -lost, economy.DirtyCash));
            Emit(new BattleEvent(BattleEventType.Result, "Du wirst abgeführt. Ab in den Knast …"));
        }

        private bool TryFire(Trigger trigger, string errorMessage)
        {
            if (!machine.CanFire(trigger)) return Reject(errorMessage);
            machine.Fire(trigger);
            return true;
        }

        private bool Reject(string message)
        {
            InvalidAction?.Invoke(message);
            return false;
        }

        private void Emit(BattleEvent battleEvent) => EventRaised?.Invoke(battleEvent);

        private void Emit(BattleEventType type, string message, Side? target = null) =>
            Emit(new BattleEvent(type, message, target));
    }
}
