using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;
using Side = GhettoBeasts.Combat.Side;

namespace GhettoBeasts.UI
{
    /// <summary>Pre-fight "Käfig" screen: pick a card, equip CRISPR genes, pick an opponent, launder money.</summary>
    public partial class SetupPanel : Control
    {
        [Signal] public delegate void StartRequestedEventHandler(string cardId, string[] mutationIds, string opponentId);

        private const int LaunderAmount = 100;

        private readonly Dictionary<MutationSlot, OptionButton> mutationPickers = new();
        private OptionButton cardPicker = null!;
        private OptionButton opponentPicker = null!;
        private RichTextLabel preview = null!;
        private Label walletLabel = null!;
        private Label recordLabel = null!;
        private Label errorLabel = null!;
        private Button launderButton = null!;

        private static GameManager Game => GameManager.Instance;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();

            var dim = new ColorRect { Color = new Color(0.02f, 0f, 0.05f, 0.45f), MouseFilter = MouseFilterEnum.Ignore };
            dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(dim);

            var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
            scroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(scroll);

            var center = new CenterContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
            scroll.AddChild(center);

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(Mathf.Min(620f, GetViewportRect().Size.X - 24f), 0f) };
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.04f, 0.02f, 0.09f, 0.94f), Palette.NeonPink, 2, 10, 20));
            center.AddChild(panel);

            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 10);
            panel.AddChild(box);

            box.AddChild(UiKit.Label("UNDERGROUND ARENA", 34, Palette.NeonPink, HorizontalAlignment.Center));
            var subtitle = UiKit.Label("Wähle deine Karte, verpasse ihr CRISPR-Gene und schmettere sie auf den Asphalt.", 14, Palette.TextDim, HorizontalAlignment.Center);
            subtitle.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            box.AddChild(subtitle);

            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 12);
            grid.AddThemeConstantOverride("v_separation", 8);
            box.AddChild(grid);

            cardPicker = AddPickerRow(grid, "Deine Karte");
            FillCardPicker(cardPicker);
            cardPicker.ItemSelected += _ => OnSelectionChanged();

            foreach (MutationSlot slot in Enum.GetValues<MutationSlot>())
            {
                var picker = AddPickerRow(grid, $"CRISPR {slot.DisplayName()}");
                picker.AddItem("— leer —");
                picker.SetItemMetadata(0, string.Empty);
                foreach (var mutation in ContentCatalog.MutationsForSlot(slot))
                {
                    picker.AddItem($"{mutation.Name}  (+{mutation.Instability}% Gen)");
                    picker.SetItemMetadata(picker.ItemCount - 1, mutation.Id);
                    picker.SetItemTooltip(picker.ItemCount - 1, mutation.Description);
                }
                picker.ItemSelected += _ => OnSelectionChanged();
                mutationPickers[slot] = picker;
            }

            opponentPicker = AddPickerRow(grid, "Gegner");
            FillCardPicker(opponentPicker);
            opponentPicker.ItemSelected += _ => OnSelectionChanged();

            preview = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false, CustomMinimumSize = new Vector2(0f, 120f) };
            preview.AddThemeFontSizeOverride("normal_font_size", 14);
            var previewPanel = new PanelContainer();
            previewPanel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.02f, 0.01f, 0.05f, 0.9f), Palette.NeonPurple.Darkened(0.3f), 1, 6, 10));
            previewPanel.AddChild(preview);
            box.AddChild(previewPanel);

            var walletRow = new HBoxContainer();
            walletRow.AddThemeConstantOverride("separation", 10);
            walletLabel = UiKit.Label("", 14, Palette.DirtyCash);
            walletLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            walletRow.AddChild(walletLabel);
            launderButton = UiKit.Button($"Waschsalon: {LaunderAmount} $ waschen", $"{LaunderAmount} $ Dirty → {(int)(LaunderAmount * (1 - Economy.LaunderFee))} $ Clean ({Economy.LaunderFee:P0} Gebühr)");
            launderButton.CustomMinimumSize = new Vector2(200f, 38f);
            launderButton.Pressed += OnLaunderPressed;
            walletRow.AddChild(launderButton);
            box.AddChild(walletRow);

            recordLabel = UiKit.Label("", 12, Palette.TextDim);
            box.AddChild(recordLabel);

            errorLabel = UiKit.Label("", 13, Palette.Danger);
            errorLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            errorLabel.Visible = false;
            box.AddChild(errorLabel);

            var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            buttons.AddThemeConstantOverride("separation", 10);
            var reset = UiKit.Button("Spielstand zurücksetzen");
            reset.CustomMinimumSize = new Vector2(170f, 44f);
            reset.Pressed += OnResetPressed;
            var start = UiKit.Button("KARTE AUSSPIELEN!");
            start.CustomMinimumSize = new Vector2(240f, 52f);
            start.AddThemeFontSizeOverride("font_size", 19);
            start.AddThemeStyleboxOverride("normal", UiKit.Box(new Color(0.45f, 0.05f, 0.35f, 1f), Palette.NeonPink, 2, 8, 10));
            start.Pressed += OnStartPressed;
            buttons.AddChild(reset);
            buttons.AddChild(start);
            box.AddChild(buttons);

            box.AddChild(UiKit.Label("Tastatur im Kampf: 1 Angriff · 2 Skill · 3 Backstein · 4 Doping · 5 Schiri", 12, Palette.TextDim, HorizontalAlignment.Center));

            Game.CurrencyChanged += OnCurrencyChanged;
            RestoreSelection();
            Refresh();
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance is not null) Game.CurrencyChanged -= OnCurrencyChanged;
        }

        private void OnCurrencyChanged(int currency, int value) => RefreshWallet();

        public void Refresh()
        {
            RefreshWallet();
            OnSelectionChanged();
        }

        public void ShowError(string message)
        {
            errorLabel.Text = message;
            errorLabel.Visible = true;
        }

        private OptionButton AddPickerRow(GridContainer grid, string label)
        {
            grid.AddChild(UiKit.Label(label, 14, Palette.TextDim));
            var picker = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill, FitToLongestItem = false, CustomMinimumSize = new Vector2(0f, 38f) };
            grid.AddChild(picker);
            return picker;
        }

        private static void FillCardPicker(OptionButton picker)
        {
            AddGrouped(picker, "★ Hero-Bestien", ContentCatalog.Beasts.Take(8));
            AddGrouped(picker, "Straßen-Bestien", ContentCatalog.Beasts.Skip(8));
            AddGrouped(picker, "★ Hero-NPCs", ContentCatalog.Npcs.Take(7));
            AddGrouped(picker, "Straßen-Söldner", ContentCatalog.Npcs.Skip(7));
        }

        private static void AddGrouped(OptionButton picker, string title, IEnumerable<CardDefinition> cards)
        {
            picker.AddSeparator(title);
            foreach (var card in cards) AddCard(picker, card);
        }

        private static void AddCard(OptionButton picker, CardDefinition card)
        {
            picker.AddItem($"{card.Name}  [{card.Rarity.DisplayName()}]");
            picker.SetItemMetadata(picker.ItemCount - 1, card.Id);
            picker.SetItemTooltip(picker.ItemCount - 1, card.Description);
        }

        private static void SelectById(OptionButton picker, string id)
        {
            for (int i = 0; i < picker.ItemCount; i++)
            {
                if (picker.IsItemSeparator(i) || picker.GetItemMetadata(i).AsString() != id) continue;
                picker.Select(i);
                return;
            }
            for (int i = 0; i < picker.ItemCount; i++)
            {
                if (picker.IsItemSeparator(i)) continue;
                picker.Select(i);
                return;
            }
        }

        private static string SelectedId(OptionButton picker) =>
            picker.Selected >= 0 ? picker.GetItemMetadata(picker.Selected).AsString() : string.Empty;

        private void RestoreSelection()
        {
            var save = Game.Save;
            SelectById(cardPicker, save.SelectedCardId);
            SelectById(opponentPicker, save.LastOpponentId);
            foreach (var picker in mutationPickers.Values) picker.Select(0);
            foreach (var id in save.EquippedMutationIds)
            {
                var mutation = ContentCatalog.Mutations.FirstOrDefault(m => m.Id == id);
                if (mutation is not null) SelectById(mutationPickers[mutation.Slot], id);
            }
        }

        private string[] SelectedMutationIds() =>
            mutationPickers.Values.Where(p => !p.Disabled).Select(SelectedId).Where(id => id.Length > 0).ToArray();

        private void OnSelectionChanged()
        {
            errorLabel.Visible = false;
            if (!ContentCatalog.TryGetCard(SelectedId(cardPicker), out var card)) return;

            foreach (var picker in mutationPickers.Values)
            {
                picker.Disabled = !card.CanMutate;
                picker.TooltipText = card.CanMutate ? string.Empty : "NPCs können nicht mutiert werden.";
            }

            var mutations = SelectedMutationIds().Select(ContentCatalog.GetMutation).ToList();
            Combatant combatant;
            try
            {
                combatant = Combatant.Create(Side.Player, card, mutations);
            }
            catch (InvalidOperationException ex)
            {
                ShowError(ex.Message);
                return;
            }

            var stats = combatant.Stats;
            var rarity = Palette.ForRarity(card.Rarity);
            int instability = combatant.Instability;
            string instabilityHint = instability >= 100 ? "Spontane Mutation oder Kollaps möglich!"
                : instability > InstabilityResolver.CriticalThreshold ? "35% Risiko: Zellausfall oder Zorn-Modus."
                : instability > InstabilityResolver.ConfusionThreshold ? "15% Risiko: Verwirrung."
                : "Stabil.";
            string instabilityColor = UiKit.Hex(instability > InstabilityResolver.CriticalThreshold ? Palette.Danger
                : instability > InstabilityResolver.ConfusionThreshold ? Palette.NeonPink : Palette.NeonPurple);

            ContentCatalog.TryGetCard(SelectedId(opponentPicker), out var opponent);
            preview.Text =
                $"[color=#{UiKit.Hex(rarity)}][b]{card.Name}[/b]  ·  {card.Rarity.DisplayName()}[/color]  [color=#{UiKit.Hex(Palette.TextDim)}]{card.Category}[/color]\n" +
                $"{card.Description}\n" +
                $"[color=#{UiKit.Hex(Palette.NeonCyan)}]HP {stats.MaxHp}  ·  ATK {stats.Attack}  ·  DEF {stats.Defense}  ·  INIT {stats.Speed}  ·  Ausweichen {stats.Evasion}%  ·  Krit {stats.CritChance}%[/color]\n" +
                $"[color=#{UiKit.Hex(Palette.NeonPink)}]Skill: {card.AbilityName}[/color] – {card.AbilityDescription}\n" +
                $"[color=#{instabilityColor}]Gen-Instabilität {instability}% – {instabilityHint}[/color]" +
                (opponent is null ? string.Empty
                    : $"\n[color=#{UiKit.Hex(Palette.TextDim)}]Gegner: {opponent.Name} ({opponent.Rarity.DisplayName()}) · Sieg bringt {ContentCatalog.DirtyCashReward(opponent.Rarity)} $[/color]");
        }

        private void RefreshWallet()
        {
            var economy = Game.Economy;
            walletLabel.Text = $"$ {economy.DirtyCash} dirty   ·   $ {economy.CleanCash} clean   ·   ◆ {economy.Diamonds}";
            launderButton.Disabled = economy.DirtyCash < LaunderAmount;
            var save = Game.Save;
            recordLabel.Text = $"Bilanz: {save.Wins} Siege · {save.Losses} Niederlagen · {save.Raids} Razzien";
        }

        private void OnLaunderPressed()
        {
            int clean = Game.Economy.Launder(LaunderAmount);
            if (clean <= 0)
            {
                ShowError("Nicht genug Schmutzgeld zum Waschen.");
                return;
            }
            Game.Persist();
            RefreshWallet();
        }

        private void OnResetPressed()
        {
            Game.ResetProgress();
            RestoreSelection();
            Refresh();
        }

        private void OnStartPressed()
        {
            string cardId = SelectedId(cardPicker);
            string opponentId = SelectedId(opponentPicker);
            if (cardId.Length == 0 || opponentId.Length == 0)
            {
                ShowError("Bitte Karte und Gegner wählen.");
                return;
            }
            EmitSignal(SignalName.StartRequested, cardId, SelectedMutationIds(), opponentId);
        }
    }
}
