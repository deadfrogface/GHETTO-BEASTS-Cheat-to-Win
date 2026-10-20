using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>Autoload singleton holding the persistent economy and save state.</summary>
    public partial class GameManager : Node
    {
        [Signal] public delegate void CurrencyChangedEventHandler(int currency, int value);

        public static GameManager Instance { get; private set; } = null!;

        public Economy Economy { get; private set; } = new();
        public SaveData Save { get; private set; } = new();
        public IRandom Random { get; } = new SystemRandom();

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            LoadState(SaveSystem.Load());
            GD.Print($"[GameManager] Spielstand geladen: {Economy.DirtyCash} $ dirty, {Economy.CleanCash} $ clean, {Economy.Diamonds} Diamonds.");
        }

        public void Persist()
        {
            Save.DirtyCash = Economy.DirtyCash;
            Save.CleanCash = Economy.CleanCash;
            Save.Diamonds = Economy.Diamonds;
            SaveSystem.Write(Save);
        }

        public void ResetProgress()
        {
            SaveSystem.Delete();
            LoadState(new SaveData());
            Persist();
        }

        private void LoadState(SaveData data)
        {
            if (Economy is not null) Economy.Changed -= OnEconomyChanged;
            Save = data;
            Economy = new Economy(data.DirtyCash, data.CleanCash, data.Diamonds);
            Economy.Changed += OnEconomyChanged;
            EmitSignal(SignalName.CurrencyChanged, (int)Currency.DirtyCash, Economy.DirtyCash);
        }

        private void OnEconomyChanged(Currency currency, int value) =>
            EmitSignal(SignalName.CurrencyChanged, (int)currency, value);
    }
}
