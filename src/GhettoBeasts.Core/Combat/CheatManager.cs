using System;
using GhettoBeasts.Core;

namespace GhettoBeasts.Combat
{
    public readonly record struct CheatResult(bool Executed, bool Success, bool TrainerKnockedOut);

    public sealed class CheatManager
    {
        public const int BrickSuspicion = 35;
        public const double BrickHitChance = 20;

        public const int DopingCost = 60;
        public const int DopingSuspicion = 20;
        public const int DopingTurns = 3;
        public const int DopingInstability = 8;

        public const int BribeCost = 120;
        public const int BribeReduction = 35;
        public const double BribeFailChance = 15;
        public const int BribeFailSuspicion = 15;

        private readonly Economy economy;
        private readonly SuspicionMeter suspicion;
        private readonly IRandom random;
        private readonly CombatModifiers modifiers;

        public CheatManager(Economy economy, SuspicionMeter suspicion, IRandom random, CombatModifiers? modifiers = null)
        {
            this.economy = economy ?? throw new ArgumentNullException(nameof(economy));
            this.suspicion = suspicion ?? throw new ArgumentNullException(nameof(suspicion));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.modifiers = modifiers ?? CombatModifiers.None;
        }

        public static int CostOf(CheatType cheat) => cheat switch
        {
            CheatType.Brick => 0,
            CheatType.Doping => DopingCost,
            CheatType.Bribe => BribeCost,
            _ => throw new ArgumentOutOfRangeException(nameof(cheat))
        };

        public static string DisplayName(CheatType cheat) => cheat switch
        {
            CheatType.Brick => "Backstein-Wurf",
            CheatType.Doping => "Illegale Injektion",
            CheatType.Bribe => "Schiri-Bestechung",
            _ => cheat.ToString()
        };

        public bool CanAfford(CheatType cheat)
        {
            if (cheat == CheatType.Bribe && modifiers.RefFlatrate)
                return economy.CanAfford(Currency.CleanCash, modifiers.RefFlatrateCleanCost);
            return economy.CanAfford(Currency.DirtyCash, CostOf(cheat));
        }

        public CheatResult Execute(CheatType cheat, Combatant player, Combatant enemy, Action<BattleEvent> emit)
        {
            if (cheat == CheatType.Bribe && modifiers.RefFlatrate)
            {
                if (!economy.TrySpend(Currency.CleanCash, modifiers.RefFlatrateCleanCost))
                {
                    emit(new BattleEvent(BattleEventType.Log,
                        $"Zu wenig Clean-Cash für Schiri-Flatrate ({modifiers.RefFlatrateCleanCost} $)."));
                    return new CheatResult(false, false, false);
                }
                emit(new BattleEvent(BattleEventType.Cash,
                    $"-{modifiers.RefFlatrateCleanCost} $ Clean (Flatrate)",
                    Side.Player, -modifiers.RefFlatrateCleanCost, economy.CleanCash));
                return BribeReferee(emit);
            }

            if (!economy.TrySpend(Currency.DirtyCash, CostOf(cheat)))
            {
                emit(new BattleEvent(BattleEventType.Log, $"Zu wenig Schmutzgeld für {DisplayName(cheat)} ({CostOf(cheat)} $)."));
                return new CheatResult(false, false, false);
            }

            if (CostOf(cheat) > 0)
                emit(new BattleEvent(BattleEventType.Cash, $"-{CostOf(cheat)} $ Schmutzgeld", Side.Player, -CostOf(cheat), economy.DirtyCash));

            return cheat switch
            {
                CheatType.Brick => ThrowBrick(enemy, emit),
                CheatType.Doping => InjectDoping(player, emit),
                CheatType.Bribe => BribeReferee(emit),
                _ => throw new ArgumentOutOfRangeException(nameof(cheat))
            };
        }

        private CheatResult ThrowBrick(Combatant enemy, Action<BattleEvent> emit)
        {
            bool hit = random.Chance(BrickHitChance);
            emit(new BattleEvent(BattleEventType.Cheat,
                hit ? "BACKSTEIN! Der gegnerische Trainer geht K.O.!" : "Der Backstein verfehlt den gegnerischen Trainer knapp.",
                Side.Enemy, Critical: hit, Cheat: CheatType.Brick));
            RaiseSuspicion(BrickSuspicion, emit);
            if (hit) enemy.Knockout();
            return new CheatResult(true, hit, hit);
        }

        private CheatResult InjectDoping(Combatant player, Action<BattleEvent> emit)
        {
            player.Apply(StatusType.Doped, DopingTurns);
            player.ChangeInstability(DopingInstability);
            emit(new BattleEvent(BattleEventType.Cheat, $"Illegale Injektion! {player.Name} ist gedopt (+50% Angriff).",
                Side.Player, Critical: true, Cheat: CheatType.Doping, Status: StatusType.Doped));
            emit(new BattleEvent(BattleEventType.Instability, $"Instabilität +{DopingInstability}%",
                Side.Player, DopingInstability, player.Instability));
            RaiseSuspicion(DopingSuspicion, emit);
            return new CheatResult(true, true, false);
        }

        private CheatResult BribeReferee(Action<BattleEvent> emit)
        {
            if (random.Chance(BribeFailChance))
            {
                emit(new BattleEvent(BattleEventType.Cheat, "Der Schiri nimmt das Geld – und meldet dich trotzdem!",
                    null, Critical: false, Cheat: CheatType.Bribe));
                RaiseSuspicion(BribeFailSuspicion, emit);
                return new CheatResult(true, false, false);
            }

            emit(new BattleEvent(BattleEventType.Cheat, "Der Schiri steckt die Scheine ein und schaut weg.",
                null, Critical: true, Cheat: CheatType.Bribe));
            int before = suspicion.Value;
            suspicion.Reduce(BribeReduction);
            emit(new BattleEvent(BattleEventType.Suspicion, $"SWAT-Verdacht -{before - suspicion.Value}%",
                null, suspicion.Value - before, suspicion.Value));
            return new CheatResult(true, true, false);
        }

        private void RaiseSuspicion(int amount, Action<BattleEvent> emit)
        {
            int scaled = Math.Max(1, (int)Math.Round(amount * modifiers.SuspicionGainScale));
            int before = suspicion.Value;
            suspicion.Add(scaled);
            string note = modifiers.SuspicionGainScale < 1.0 ? " (Anwalt)" : "";
            emit(new BattleEvent(BattleEventType.Suspicion, $"SWAT-Verdacht +{suspicion.Value - before}%{note}",
                null, suspicion.Value - before, suspicion.Value));
        }
    }
}
