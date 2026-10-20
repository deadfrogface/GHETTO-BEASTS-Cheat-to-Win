using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Data;

namespace GhettoBeasts.Combat
{
    public enum Side
    {
        Player,
        Enemy
    }

    public enum StatusType
    {
        Bleed,
        Poison,
        Confused,
        Stunned,
        Banished,
        Doped,
        Enraged
    }

    public sealed class Combatant
    {
        public const int MaxInstability = 100;

        private readonly Dictionary<StatusType, int> statuses = new();
        private readonly Dictionary<MutationSlot, MutationDefinition> mutations;

        private Combatant(Side side, CardDefinition definition, Dictionary<MutationSlot, MutationDefinition> mutations)
        {
            Side = side;
            Definition = definition;
            this.mutations = mutations;
            Stats = mutations.Values.Aggregate(definition.BaseStats, (s, m) => s + m.StatBonus);
            Hp = Stats.MaxHp;
            Instability = Math.Min(MaxInstability, definition.BaseInstability + mutations.Values.Sum(m => m.Instability));
        }

        public Side Side { get; }
        public CardDefinition Definition { get; }
        public string Name => Definition.Name;
        public Stats Stats { get; }
        public int Hp { get; private set; }
        public int Instability { get; private set; }
        public int SkillCooldown { get; internal set; }
        public bool HasMutated { get; internal set; }
        public bool IsDown => Hp <= 0;
        public bool SkillReady => SkillCooldown <= 0;
        public IReadOnlyDictionary<MutationSlot, MutationDefinition> Mutations => mutations;
        public IReadOnlyDictionary<StatusType, int> Statuses => statuses;

        public int EffectiveAttack
        {
            get
            {
                double attack = Stats.Attack;
                if (Has(StatusType.Doped)) attack *= 1.5;
                if (Has(StatusType.Enraged)) attack *= 1.3;
                if (HasMutated) attack *= 1.4;
                return (int)Math.Round(attack);
            }
        }

        public static Combatant Create(Side side, CardDefinition definition, IEnumerable<MutationDefinition>? mutations = null)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));
            var list = mutations?.ToList() ?? new List<MutationDefinition>();
            if (list.Count > 0 && !definition.CanMutate)
                throw new InvalidOperationException($"{definition.Name} ist ein NPC und kann nicht mutiert werden.");

            var bySlot = new Dictionary<MutationSlot, MutationDefinition>();
            foreach (var mutation in list)
            {
                if (bySlot.ContainsKey(mutation.Slot))
                    throw new InvalidOperationException($"Slot {mutation.Slot.DisplayName()} ist doppelt belegt.");
                bySlot[mutation.Slot] = mutation;
            }
            return new Combatant(side, definition, bySlot);
        }

        public static int PreviewInstability(CardDefinition definition, IEnumerable<MutationDefinition> mutations) =>
            Math.Min(MaxInstability, definition.BaseInstability + mutations.Sum(m => m.Instability));

        /// <summary>Used by CRISPR lab stabilizers after construction.</summary>
        public void ForceInstability(int value) =>
            Instability = Math.Max(0, Math.Min(MaxInstability, value));

        public bool HasEffect(MutationEffect effect) => mutations.Values.Any(m => m.Effect == effect);

        public bool Has(StatusType status) => statuses.TryGetValue(status, out int turns) && turns > 0;

        internal void Apply(StatusType status, int turns)
        {
            statuses[status] = Math.Max(turns, statuses.TryGetValue(status, out int current) ? current : 0);
        }

        internal void Remove(StatusType status) => statuses.Remove(status);

        internal void TickStatuses()
        {
            foreach (var key in statuses.Keys.ToList())
            {
                statuses[key]--;
                if (statuses[key] <= 0) statuses.Remove(key);
            }
        }

        /// <returns>Damage actually dealt.</returns>
        internal int TakeDamage(int amount)
        {
            int dealt = Math.Clamp(amount, 0, Hp);
            Hp -= dealt;
            return dealt;
        }

        /// <returns>HP actually restored.</returns>
        internal int Heal(int amount)
        {
            int healed = Math.Clamp(amount, 0, Stats.MaxHp - Hp);
            Hp += healed;
            return healed;
        }

        internal void Knockout() => Hp = 0;

        internal void ChangeInstability(int delta) => Instability = Math.Clamp(Instability + delta, 0, MaxInstability);
    }
}
