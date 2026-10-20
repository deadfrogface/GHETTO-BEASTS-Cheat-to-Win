using GhettoBeasts.Data;
using Godot;

namespace GhettoBeasts.Core
{
    /// <summary>Autoload singleton holding the persistent economy and save state.</summary>
    public partial class GameManager : Node
    {
        public const string HubScene = "res://scenes/hub/hub.tscn";
        public const string DistrictScene = "res://scenes/district/district.tscn";
        public const string CombatScene = "res://scenes/combat/combat.tscn";
        public const string LabScene = "res://scenes/buildings/crispr_lab.tscn";
        public const string WashScene = "res://scenes/buildings/wash.tscn";
        public const string GachaScene = "res://scenes/buildings/darknet.tscn";
        public const string DopeScene = "res://scenes/buildings/dope_shop.tscn";
        public const string WeapScene = "res://scenes/buildings/weap_shop.tscn";

        [Signal] public delegate void CurrencyChangedEventHandler(int currency, int value);

        public static GameManager Instance { get; private set; } = null!;

        public Economy Economy { get; private set; } = new();
        public SaveData Save { get; private set; } = SaveData.CreateDefault();
        public IRandom Random { get; } = new SystemRandom();

        /// <summary>Scene to return to after combat (hub or district).</summary>
        public string ReturnScenePath { get; set; } = HubScene;

        /// <summary>Optional forced opponent when entering combat from stealth detection.</summary>
        public string? PendingOpponentId { get; set; }

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            LoadState(SaveSystem.Load());
            var catalog = DatabaseCatalog.Instance.Snapshot();
            GD.Print($"[GameManager] Spielstand geladen: {Economy.DirtyCash} $ dirty, {Economy.CleanCash} $ clean, {Economy.Diamonds} Diamonds.");
            GD.Print($"[GameManager] Roster: {Save.OwnedCardIds.Count} Karten, {Save.OwnedMutationIds.Count} Gene.");
            GD.Print($"[GameManager] DatabaseCatalog: {catalog.Cards} Karten ({catalog.Beasts} Bestien / {catalog.Npcs} NPCs), {catalog.Mutations} Mutationen.");

            if (WantsAutoBattle())
            {
                ReturnScenePath = CombatScene;
                CallDeferred(nameof(GoToCombatDeferred));
            }
        }

        public void Persist()
        {
            Save.DirtyCash = Economy.DirtyCash;
            Save.CleanCash = Economy.CleanCash;
            Save.Diamonds = Economy.Diamonds;
            Roster.SyncLegacyEquipped(Save);
            SaveSystem.Write(Save);
        }

        public void ResetProgress()
        {
            SaveSystem.Delete();
            LoadState(SaveData.CreateDefault());
            Persist();
        }

        public void GoTo(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath)) return;
            var err = GetTree().ChangeSceneToFile(scenePath);
            if (err != Error.Ok)
                GD.PrintErr($"[GameManager] Szenewechsel fehlgeschlagen: {scenePath} ({err})");
        }

        public void EnterCombat(string? returnScene = null, string? forcedOpponent = null)
        {
            ReturnScenePath = returnScene ?? HubScene;
            PendingOpponentId = forcedOpponent;
            GoTo(CombatScene);
        }

        private void GoToCombatDeferred() => GoTo(CombatScene);

        private static bool WantsAutoBattle()
        {
            foreach (var arg in OS.GetCmdlineUserArgs())
            {
                if (arg.Equals("--autobattle", System.StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("--autobattle=", System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void LoadState(SaveData data)
        {
            if (Economy is not null) Economy.Changed -= OnEconomyChanged;
            Roster.EnsureDefaults(data);
            Save = data;
            Economy = new Economy(data.DirtyCash, data.CleanCash, data.Diamonds);
            Economy.Changed += OnEconomyChanged;
            EmitSignal(SignalName.CurrencyChanged, (int)Currency.DirtyCash, Economy.DirtyCash);
        }

        private void OnEconomyChanged(Currency currency, int value) =>
            EmitSignal(SignalName.CurrencyChanged, (int)currency, value);
    }
}
