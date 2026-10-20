using System;
using GhettoBeasts.Core;
using GhettoBeasts.Data;

namespace GhettoBeasts.Combat
{
    public sealed partial class BattleEngine
    {
        private readonly record struct HitOptions(bool TrueStrike = false, bool ForceCrit = false);

        private void ExecuteAction(Combatant actor, Combatant opponent, ActionKind kind)
        {
            ApplyTurnStartEffects(actor);

            if (!actor.IsDown)
            {
                if (actor.Has(StatusType.Banished))
                    Emit(BattleEventType.Log, $"{actor.Name} hängt noch auf der Hand fest und setzt aus.", actor.Side);
                else if (actor.Has(StatusType.Stunned))
                    Emit(BattleEventType.Log, $"{actor.Name} ist betäubt und kann sich nicht rühren!", actor.Side);
                else
                    PerformWithInstability(actor, opponent, kind);
            }

            actor.TickStatuses();
            if (actor.SkillCooldown > 0) actor.SkillCooldown--;
            actionsTaken++;
        }

        private void ApplyTurnStartEffects(Combatant actor)
        {
            if (actor.Has(StatusType.Bleed))
                DamageOverTime(actor, Math.Max(2, actor.Stats.MaxHp * 6 / 100), "blutet");
            if (!actor.IsDown && actor.Has(StatusType.Poison))
                DamageOverTime(actor, Math.Max(3, actor.Stats.MaxHp * 8 / 100), "leidet unter Gift");
            if (!actor.IsDown && actor.Definition.Traits.HasFlag(CardTraits.Regenerates))
                HealSelf(actor, actor.Stats.MaxHp * 8 / 100, "regeneriert Zellgewebe");
        }

        private void PerformWithInstability(Combatant actor, Combatant opponent, ActionKind kind)
        {
            switch (InstabilityResolver.Roll(actor.Instability, random, actor.HasMutated))
            {
                case InstabilityOutcome.Confusion:
                    actor.Apply(StatusType.Confused, 1);
                    EmitStatus(actor, StatusType.Confused, $"Gen-Instabilität! {actor.Name} ist kurz desorientiert.");
                    break;

                case InstabilityOutcome.CellFailure:
                    int selfDamage = actor.TakeDamage(Math.Max(1, actor.Stats.MaxHp * 12 / 100));
                    Emit(new BattleEvent(BattleEventType.Damage, $"ZELLAUSFALL! {actor.Name} verliert {selfDamage} HP.",
                        actor.Side, selfDamage, actor.Hp));
                    if (actor.IsDown) return;
                    break;

                case InstabilityOutcome.Rage:
                    actor.Apply(StatusType.Enraged, 2);
                    EmitStatus(actor, StatusType.Enraged, $"ZORN-MODUS! {actor.Name} rastet aus und greift wild um sich.");
                    if (random.Chance(50))
                    {
                        Emit(BattleEventType.Log, $"{actor.Name} beißt in blinder Wut sich selbst!", actor.Side);
                        Hit(actor, actor, 60);
                        return;
                    }
                    break;

                case InstabilityOutcome.WildMutation:
                    actor.HasMutated = true;
                    actor.Apply(StatusType.Enraged, 2);
                    int healed = actor.Heal(actor.Stats.MaxHp * 20 / 100);
                    Emit(new BattleEvent(BattleEventType.Instability,
                        $"SPONTANE MUTATION! {actor.Name} wächst zu einer unkontrollierbaren Monstrosität (+40% Angriff).",
                        actor.Side, 0, actor.Instability, Critical: true));
                    if (healed > 0)
                        Emit(new BattleEvent(BattleEventType.Heal, $"{actor.Name} regeneriert {healed} HP.", actor.Side, healed, actor.Hp));
                    break;

                case InstabilityOutcome.Collapse:
                    int remaining = actor.Hp;
                    actor.Knockout();
                    Emit(new BattleEvent(BattleEventType.Damage, $"KOLLAPS! {actor.Name}s Gene zerfallen – K.O.!",
                        actor.Side, remaining, 0, Critical: true));
                    return;
            }

            if (actor.Has(StatusType.Confused))
            {
                double roll = random.NextDouble();
                if (roll < 0.5)
                {
                    Emit(BattleEventType.Log, $"{actor.Name} ist verwirrt und greift sich selbst an!", actor.Side);
                    Hit(actor, actor, 60);
                    return;
                }
                if (roll < 0.75)
                {
                    Emit(BattleEventType.Log, $"{actor.Name} ist verwirrt und vergisst seinen Zug.", actor.Side);
                    return;
                }
            }

            if (kind == ActionKind.Skill && actor.SkillReady)
                UseAbility(actor, opponent);
            else
            {
                Emit(new BattleEvent(BattleEventType.Attack, $"{actor.Name} greift an!", actor.Side));
                Hit(actor, opponent, BasicAttackPower);
            }
        }

        private void UseAbility(Combatant actor, Combatant target)
        {
            var definition = actor.Definition;
            actor.SkillCooldown = SkillCooldownTurns + 1;
            Emit(new BattleEvent(BattleEventType.SkillUsed, $"{actor.Name} setzt {definition.AbilityName} ein!", actor.Side));

            switch (definition.Ability)
            {
                case AbilityId.ToxicSpit:
                    if (Hit(actor, target, 60)) ApplyStatus(target, StatusType.Poison, 3, "ist vergiftet");
                    break;

                case AbilityId.JawLock:
                    if (Hit(actor, target, 70)) ApplyStatus(target, StatusType.Stunned, 1, "steckt in der Kiefersperre fest");
                    break;

                case AbilityId.Frenzy:
                    Hit(actor, target, 55);
                    if (!target.IsDown && !actor.IsDown) Hit(actor, target, 55);
                    break;

                case AbilityId.MadLaugh:
                    ApplyStatus(target, StatusType.Confused, 2, "ist vom irren Lachen verwirrt");
                    StealCash(actor, 40);
                    break;

                case AbilityId.DeathRoll:
                    if (Hit(actor, target, 120)) ApplyStatus(target, StatusType.Stunned, 1, "wird unter Wasser gezogen");
                    break;

                case AbilityId.ChestPound:
                    if (Hit(actor, target, 90) && random.Chance(60))
                        ApplyStatus(target, StatusType.Stunned, 1, "ist vom Brusttrommeln betäubt");
                    break;

                case AbilityId.LaserEyes:
                    Hit(actor, target, 130, new HitOptions(TrueStrike: true));
                    break;

                case AbilityId.AcidSpray:
                    Hit(actor, target, 75, new HitOptions(TrueStrike: true));
                    ApplyStatus(target, StatusType.Poison, 3, "wird von Säure zerfressen");
                    break;

                case AbilityId.DirtyNeedle:
                    if (Hit(actor, target, random.Range(30, 161)) && random.Chance(30))
                        ApplyStatus(target, StatusType.Poison, 3, "hat sich etwas eingefangen");
                    break;

                case AbilityId.WrenchTuning:
                    int percent = actor.Definition.IsCyber ? 40 : 25;
                    HealSelf(actor, actor.Stats.MaxHp * percent / 100, "wird zusammengeschraubt");
                    ApplyStatus(actor, StatusType.Doped, 2, "läuft auf Tuning");
                    break;

                case AbilityId.Seduction:
                    ApplyStatus(target, StatusType.Confused, 2, "ist völlig betört");
                    break;

                case AbilityId.HouseBan:
                    target.Remove(StatusType.Doped);
                    target.Remove(StatusType.Enraged);
                    ApplyStatus(target, StatusType.Banished, 1, "kassiert Hausverbot und fliegt zurück auf die Hand");
                    break;

                case AbilityId.BackalleySurgery:
                    actor.Remove(StatusType.Poison);
                    actor.Remove(StatusType.Bleed);
                    HealSelf(actor, actor.Stats.MaxHp * 35 / 100, "wird im Hinterhof zusammengeflickt");
                    if (actor.Instability > 0)
                    {
                        actor.ChangeInstability(-25);
                        Emit(new BattleEvent(BattleEventType.Instability, $"{actor.Name}s Gene stabilisieren sich (-25%).",
                            actor.Side, -25, actor.Instability));
                    }
                    break;

                case AbilityId.DeathDance:
                    if (Hit(actor, target, 85, new HitOptions(ForceCrit: true)))
                        ApplyStatus(target, StatusType.Bleed, 3, "blutet aus tiefen Schnitten");
                    break;

                case AbilityId.CallBodyguards:
                    Hit(actor, target, 60);
                    if (!target.IsDown && !actor.IsDown) Hit(actor, target, 60);
                    int before = Suspicion.Value;
                    Suspicion.Reduce(25);
                    if (before != Suspicion.Value)
                        Emit(new BattleEvent(BattleEventType.Suspicion, $"El Capo schmiert das SWAT-Team: Verdacht -{before - Suspicion.Value}%",
                            null, Suspicion.Value - before, Suspicion.Value));
                    break;

                default:
                    throw new InvalidOperationException($"Unbekannte Fähigkeit {definition.Ability}.");
            }
        }

        private void PartnerHit(Combatant attacker, Combatant defender, int power) =>
            Hit(attacker, defender, power);

        /// <returns>True if the hit landed.</returns>
        private bool Hit(Combatant attacker, Combatant defender, int power, HitOptions options = default)
        {
            bool selfHit = ReferenceEquals(attacker, defender);

            if (!selfHit && Round == 1 && defender.HasEffect(MutationEffect.CamouflageFirstRound))
            {
                Emit(new BattleEvent(BattleEventType.Miss, $"{defender.Name} ist getarnt – der Angriff geht ins Leere!", defender.Side));
                return false;
            }

            bool trueStrike = options.TrueStrike || attacker.HasEffect(MutationEffect.TrueStrike);
            if (!selfHit && !trueStrike && random.Chance(defender.Stats.Evasion))
            {
                Emit(new BattleEvent(BattleEventType.Miss, $"{defender.Name} weicht aus!", defender.Side));
                return false;
            }

            double raw = attacker.EffectiveAttack * (power / 100.0) * (100.0 / (100.0 + defender.Stats.Defense));
            raw *= 0.9 + random.NextDouble() * 0.2;

            bool critical = !selfHit && (options.ForceCrit || random.Chance(attacker.Stats.CritChance));
            if (critical) raw *= attacker.HasEffect(MutationEffect.CritDamage) ? 2.0 : 1.5;
            if (defender.Definition.Traits.HasFlag(CardTraits.Shielded)) raw *= 0.85;

            int dealt = defender.TakeDamage(Math.Max(1, (int)Math.Round(raw)));
            string label = critical ? "KRITISCH! " : string.Empty;
            Emit(new BattleEvent(BattleEventType.Damage, $"{label}{defender.Name} erleidet {dealt} Schaden.",
                defender.Side, dealt, defender.Hp, critical));

            if (selfHit) return true;

            if (!defender.IsDown)
            {
                if (attacker.HasEffect(MutationEffect.BleedOnHit))
                    ApplyStatus(defender, StatusType.Bleed, 2, "blutet vom Piranha-Biss");
                if (attacker.HasEffect(MutationEffect.PoisonOnHit))
                    ApplyStatus(defender, StatusType.Poison, 3, "ist mit Nervengift infiziert");
            }

            if (attacker.Definition.Ability == AbilityId.MadLaugh)
                StealCash(attacker, 15);

            if (defender.HasEffect(MutationEffect.ShockCounter) && !attacker.IsDown)
            {
                int shock = attacker.TakeDamage(Math.Max(3, dealt / 5));
                Emit(new BattleEvent(BattleEventType.Damage, $"Aal-Strom! {attacker.Name} kassiert {shock} Schock-Schaden.",
                    attacker.Side, shock, attacker.Hp));
            }

            return true;
        }

        private void DamageOverTime(Combatant target, int amount, string verb)
        {
            int dealt = target.TakeDamage(amount);
            Emit(new BattleEvent(BattleEventType.Damage, $"{target.Name} {verb} (-{dealt} HP).", target.Side, dealt, target.Hp));
        }

        private void HealSelf(Combatant target, int amount, string verb)
        {
            int healed = target.Heal(amount);
            if (healed <= 0) return;
            Emit(new BattleEvent(BattleEventType.Heal, $"{target.Name} {verb} (+{healed} HP).", target.Side, healed, target.Hp));
        }

        private void ApplyStatus(Combatant target, StatusType status, int turns, string description)
        {
            target.Apply(status, turns);
            EmitStatus(target, status, $"{target.Name} {description}!");
        }

        private void EmitStatus(Combatant target, StatusType status, string message) =>
            Emit(new BattleEvent(BattleEventType.StatusApplied, message, target.Side, Status: status));

        private void StealCash(Combatant thief, int amount)
        {
            if (thief.Side == Side.Player)
            {
                economy.Add(Currency.DirtyCash, amount);
                Emit(new BattleEvent(BattleEventType.Cash, $"{thief.Name} klaut {amount} $!", Side.Player, amount, economy.DirtyCash));
                return;
            }

            int taken = economy.TakeUpTo(Currency.DirtyCash, amount);
            if (taken > 0)
                Emit(new BattleEvent(BattleEventType.Cash, $"{thief.Name} klaut dir {taken} $!", Side.Player, -taken, economy.DirtyCash));
        }
    }
}
