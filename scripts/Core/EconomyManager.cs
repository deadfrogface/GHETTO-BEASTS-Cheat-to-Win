using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>
    /// Godot-facing wallet façade over the pure <see cref="Economy"/> rules.
    /// Prefer <see cref="GameManager.Economy"/> for gameplay; this node is for scenes that need signals.
    /// </summary>
    public partial class EconomyManager : Node
    {
        [Signal] public delegate void DirtyCashChangedEventHandler(int value);
        [Signal] public delegate void CleanCashChangedEventHandler(int value);
        [Signal] public delegate void DiamondsChangedEventHandler(int value);

        public Economy Wallet { get; private set; } = new();

        public int DirtyCash => Wallet.DirtyCash;
        public int CleanCash => Wallet.CleanCash;
        public int Diamonds => Wallet.Diamonds;

        public void Bind(Economy economy)
        {
            if (Wallet is not null) Wallet.Changed -= OnChanged;
            Wallet = economy ?? throw new System.ArgumentNullException(nameof(economy));
            Wallet.Changed += OnChanged;
            EmitAll();
        }

        public bool TrySpendDirty(int amount) => Wallet.TrySpend(Currency.DirtyCash, amount);
        public bool TrySpendClean(int amount) => Wallet.TrySpend(Currency.CleanCash, amount);
        public bool TrySpendDiamonds(int amount) => Wallet.TrySpend(Currency.Diamonds, amount);
        public void AddDirty(int amount) => Wallet.Add(Currency.DirtyCash, amount);
        public void AddClean(int amount) => Wallet.Add(Currency.CleanCash, amount);
        public void AddDiamonds(int amount) => Wallet.Add(Currency.Diamonds, amount);
        public int Launder(int dirtyAmount) => Wallet.Launder(dirtyAmount);
        public int ConfiscateDirtyCash() => Wallet.ConfiscateDirtyCash();

        private void OnChanged(Currency currency, int value)
        {
            switch (currency)
            {
                case Currency.DirtyCash:
                    EmitSignal(SignalName.DirtyCashChanged, value);
                    break;
                case Currency.CleanCash:
                    EmitSignal(SignalName.CleanCashChanged, value);
                    break;
                case Currency.Diamonds:
                    EmitSignal(SignalName.DiamondsChanged, value);
                    break;
            }
        }

        private void EmitAll()
        {
            EmitSignal(SignalName.DirtyCashChanged, DirtyCash);
            EmitSignal(SignalName.CleanCashChanged, CleanCash);
            EmitSignal(SignalName.DiamondsChanged, Diamonds);
        }
    }
}
