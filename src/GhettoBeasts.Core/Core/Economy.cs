using System;

namespace GhettoBeasts.Core
{
    public enum Currency
    {
        DirtyCash,
        CleanCash,
        Diamonds
    }

    public sealed class Economy
    {
        /// <summary>Share of dirty cash the laundromat keeps as fee.</summary>
        public const double LaunderFee = 0.2;

        private int dirtyCash;
        private int cleanCash;
        private int diamonds;

        public Economy(int dirtyCash = 0, int cleanCash = 0, int diamonds = 0)
        {
            if (dirtyCash < 0 || cleanCash < 0 || diamonds < 0)
                throw new ArgumentOutOfRangeException(nameof(dirtyCash), "Währungen dürfen nicht negativ sein.");
            this.dirtyCash = dirtyCash;
            this.cleanCash = cleanCash;
            this.diamonds = diamonds;
        }

        public event Action<Currency, int>? Changed;

        public int DirtyCash => dirtyCash;
        public int CleanCash => cleanCash;
        public int Diamonds => diamonds;

        public int Get(Currency currency) => currency switch
        {
            Currency.DirtyCash => dirtyCash,
            Currency.CleanCash => cleanCash,
            Currency.Diamonds => diamonds,
            _ => throw new ArgumentOutOfRangeException(nameof(currency))
        };

        public void Add(Currency currency, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            Set(currency, Get(currency) + amount);
        }

        public bool CanAfford(Currency currency, int amount) => Get(currency) >= amount;

        public bool TrySpend(Currency currency, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!CanAfford(currency, amount)) return false;
            Set(currency, Get(currency) - amount);
            return true;
        }

        /// <summary>Removes up to <paramref name="amount"/> and returns what was actually taken.</summary>
        public int TakeUpTo(Currency currency, int amount)
        {
            int taken = Math.Min(Math.Max(0, amount), Get(currency));
            if (taken > 0) Set(currency, Get(currency) - taken);
            return taken;
        }

        /// <summary>Converts dirty cash to clean cash minus the laundromat fee. Returns the clean cash gained.</summary>
        public int Launder(int dirtyAmount)
        {
            if (dirtyAmount <= 0 || !TrySpend(Currency.DirtyCash, dirtyAmount)) return 0;
            int clean = (int)Math.Floor(dirtyAmount * (1.0 - LaunderFee));
            Add(Currency.CleanCash, clean);
            return clean;
        }

        public int ConfiscateDirtyCash() => TakeUpTo(Currency.DirtyCash, dirtyCash);

        /// <summary>Jail fail / partial bust — loses half of current dirty cash (rounded up).</summary>
        public int ConfiscateHalfDirtyCash()
        {
            int half = (dirtyCash + 1) / 2;
            return TakeUpTo(Currency.DirtyCash, half);
        }

        private void Set(Currency currency, int value)
        {
            switch (currency)
            {
                case Currency.DirtyCash: dirtyCash = value; break;
                case Currency.CleanCash: cleanCash = value; break;
                case Currency.Diamonds: diamonds = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(currency));
            }
            Changed?.Invoke(currency, value);
        }
    }
}
