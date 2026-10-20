using System;

namespace GhettoBeasts.Combat
{
    public sealed class SuspicionMeter
    {
        public const int Max = 100;

        public int Value { get; private set; }
        public bool IsMaxed => Value >= Max;

        /// <summary>Raised with (oldValue, newValue).</summary>
        public event Action<int, int>? Changed;
        public event Action? Maxed;

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Set(Value + amount);
        }

        public void Reduce(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Set(Value - amount);
        }

        public void Reset() => Set(0);

        private void Set(int value)
        {
            int old = Value;
            bool wasMaxed = IsMaxed;
            Value = Math.Clamp(value, 0, Max);
            if (old != Value) Changed?.Invoke(old, Value);
            if (!wasMaxed && IsMaxed) Maxed?.Invoke();
        }
    }
}
