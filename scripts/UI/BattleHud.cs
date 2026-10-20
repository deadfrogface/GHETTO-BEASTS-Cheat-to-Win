using System.Threading.Tasks;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;
using Side = GhettoBeasts.Combat.Side;

namespace GhettoBeasts.UI
{
    /// <summary>Burner-phone style battle HUD. Emits signals for player input; never touches game rules directly.</summary>
    public partial class BattleHud : Control
    {
        [Signal] public delegate void AttackPressedEventHandler();
        [Signal] public delegate void SkillPressedEventHandler();
        [Signal] public delegate void CheatPressedEventHandler(int cheat);
        [Signal] public delegate void ContinuePressedEventHandler();

        private const float NarrowWidth = 900f;

        private CombatantPanel playerPanel = null!;
        private CombatantPanel enemyPanel = null!;
        private ProgressBar suspicionBar = null!;
        private Label suspicionLabel = null!;
        private Label roundLabel = null!;
        private Label dirtyLabel = null!;
        private Label cleanLabel = null!;
        private Label diamondLabel = null!;
        private RichTextLabel log = null!;
        private Button attackButton = null!;
        private Button skillButton = null!;
        private Button brickButton = null!;
        private Button dopingButton = null!;
        private Button bribeButton = null!;
        private BoxContainer bottomRow = null!;
        private Label banner = null!;
        private PanelContainer resultPanel = null!;
        private Label resultTitle = null!;
        private Label resultBody = null!;
        private Tween? suspicionTween;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            MouseFilter = MouseFilterEnum.Ignore;
            Theme = UiKit.CreateTheme();

            var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            foreach (var side in new[] { "margin_left", "margin_right", "margin_top", "margin_bottom" })
                margin.AddThemeConstantOverride(side, 12);
            AddChild(margin);

            var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            margin.AddChild(column);

            var topRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            topRow.AddThemeConstantOverride("separation", 10);
            column.AddChild(topRow);

            playerPanel = new CombatantPanel();
            topRow.AddChild(playerPanel);
            topRow.AddChild(BuildCenterPanel());
            enemyPanel = new CombatantPanel();
            topRow.AddChild(enemyPanel);

            column.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore });

            bottomRow = new BoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            bottomRow.AddThemeConstantOverride("separation", 10);
            column.AddChild(bottomRow);
            bottomRow.AddChild(BuildLogPanel());
            bottomRow.AddChild(BuildActionPanel());

            banner = UiKit.Label("", 54, Palette.NeonPink, HorizontalAlignment.Center);
            banner.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
            banner.GrowHorizontal = GrowDirection.Both;
            banner.GrowVertical = GrowDirection.Both;
            banner.AddThemeConstantOverride("outline_size", 14);
            banner.Visible = false;
            banner.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(banner);

            AddChild(BuildResultPanel());

            GetViewport().SizeChanged += ApplyResponsiveLayout;
            ApplyResponsiveLayout();
        }

        public override void _ExitTree()
        {
            GetViewport().SizeChanged -= ApplyResponsiveLayout;
        }

        public void Bind(BattleEngine engine)
        {
            playerPanel.Bind(engine.Player);
            enemyPanel.Bind(engine.Enemy);
            SetSuspicion(engine.Suspicion.Value, 0);
            log.Clear();
            HideResult();
            Refresh(engine, true);
        }

        public void Refresh(BattleEngine engine, bool busy)
        {
            roundLabel.Text = engine.IsOver ? "KAMPF VORBEI" : $"RUNDE {engine.Round}";
            playerPanel.SetStatuses(engine.Player);
            enemyPanel.SetStatuses(engine.Enemy);
            playerPanel.SetInstability(engine.Player.Instability);
            enemyPanel.SetInstability(engine.Enemy.Instability);

            bool canAct = !busy && engine.IsPlayersTurn;
            attackButton.Disabled = !(canAct && engine.CanAttack);
            skillButton.Disabled = !(canAct && engine.CanUseSkill);
            skillButton.Text = engine.Player.SkillReady
                ? $"[2] {engine.Player.Definition.AbilityName}"
                : $"[2] {engine.Player.Definition.AbilityName} ({engine.Player.SkillCooldown})";
            skillButton.TooltipText = engine.Player.Definition.AbilityDescription;
            brickButton.Disabled = !(canAct && engine.CanCheat(CheatType.Brick));
            dopingButton.Disabled = !(canAct && engine.CanCheat(CheatType.Doping));
            bribeButton.Disabled = !(canAct && engine.CanCheat(CheatType.Bribe));
        }

        public void DisableActions()
        {
            foreach (var button in new[] { attackButton, skillButton, brickButton, dopingButton, bribeButton })
                button.Disabled = true;
        }

        public void SetHp(Side side, int hp) => Panel(side).SetHp(hp);

        public void SetInstability(Side side, int value) => Panel(side).SetInstability(value);

        public void SetSuspicion(int value, int delta)
        {
            suspicionBar.Value = value;
            var color = value >= 80 ? Palette.Danger : value >= 50 ? new Color(1f, 0.55f, 0.15f) : Palette.PoliceBlue.Lightened(0.3f);
            UiKit.SetBarColor(suspicionBar, color);
            suspicionLabel.Text = $"SWAT-VERDACHT {value}%";
            suspicionLabel.AddThemeColorOverride("font_color", color);

            if (delta == 0) return;
            suspicionTween?.Kill();
            suspicionBar.Scale = Vector2.One;
            suspicionBar.PivotOffset = suspicionBar.Size / 2f;
            suspicionTween = CreateTween();
            suspicionTween.TweenProperty(suspicionBar, "scale", new Vector2(1.06f, 1.5f), 0.08);
            suspicionTween.TweenProperty(suspicionBar, "scale", Vector2.One, 0.25).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        }

        public void SetCurrencies(Economy economy)
        {
            dirtyLabel.Text = $"$ {economy.DirtyCash} dirty";
            cleanLabel.Text = $"$ {economy.CleanCash} clean";
            diamondLabel.Text = $"◆ {economy.Diamonds}";
        }

        public void Log(BattleEvent battleEvent)
        {
            var color = battleEvent.Type switch
            {
                BattleEventType.Damage => battleEvent.Critical ? Palette.NeonYellow : battleEvent.Target == Side.Player ? new Color(1f, 0.55f, 0.6f) : Colors.White,
                BattleEventType.Heal => Palette.NeonGreen,
                BattleEventType.Miss => Palette.TextDim,
                BattleEventType.StatusApplied => battleEvent.Status.HasValue ? Palette.ForStatus(battleEvent.Status.Value) : Colors.White,
                BattleEventType.SkillUsed => Palette.NeonCyan,
                BattleEventType.Instability => Palette.NeonPurple,
                BattleEventType.Cheat => new Color(1f, 0.6f, 0.2f),
                BattleEventType.Suspicion => Palette.PoliceBlue.Lightened(0.35f),
                BattleEventType.Cash => Palette.DirtyCash,
                BattleEventType.Raid => Palette.Danger,
                BattleEventType.Result => Palette.NeonPink,
                BattleEventType.Summon => Palette.NeonPink,
                _ => battleEvent.Message.StartsWith("—") ? Palette.TextDim : Colors.White
            };
            log.AppendText($"[color=#{UiKit.Hex(color)}]{EscapeBbCode(battleEvent.Message)}[/color]\n");
        }

        public void LogWarning(string message) =>
            log.AppendText($"[color=#{UiKit.Hex(Palette.NeonYellow)}]! {EscapeBbCode(message)}[/color]\n");

        public async Task ShowBannerAsync(string text, Color color, double holdSeconds = 0.9)
        {
            banner.Text = text;
            banner.AddThemeColorOverride("font_color", color);
            banner.Visible = true;
            banner.PivotOffset = banner.Size / 2f;
            banner.Scale = Vector2.One * 2.2f;
            banner.Modulate = Colors.White with { A = 0f };

            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(banner, "scale", Vector2.One, 0.25).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(banner, "modulate:a", 1f, 0.15);
            tween.Chain().TweenInterval(holdSeconds);
            tween.Chain().TweenProperty(banner, "modulate:a", 0f, 0.25);
            await ToSignal(tween, Tween.SignalName.Finished);
            banner.Visible = false;
        }

        public void ShowResult(BattleOutcome outcome, string details)
        {
            (string title, Color color) = outcome switch
            {
                BattleOutcome.Victory => ("SIEG", Palette.NeonGreen),
                BattleOutcome.Defeat => ("NIEDERLAGE", Palette.Danger),
                _ => ("BUSTED", Palette.PoliceBlue.Lightened(0.3f))
            };
            resultTitle.Text = title;
            resultTitle.AddThemeColorOverride("font_color", color);
            resultBody.Text = details;
            resultPanel.Visible = true;
            resultPanel.Modulate = Colors.White with { A = 0f };
            CreateTween().TweenProperty(resultPanel, "modulate:a", 1f, 0.3);
            DisableActions();
        }

        public void HideResult() => resultPanel.Visible = false;

        private CombatantPanel Panel(Side side) => side == Side.Player ? playerPanel : enemyPanel;

        private Control BuildCenterPanel()
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.8f };
            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 3);
            panel.AddChild(box);

            roundLabel = UiKit.Label("RUNDE 1", 13, Palette.TextDim, HorizontalAlignment.Center);
            box.AddChild(roundLabel);
            suspicionLabel = UiKit.Label("SWAT-VERDACHT 0%", 14, Palette.PoliceBlue, HorizontalAlignment.Center);
            box.AddChild(suspicionLabel);
            suspicionBar = UiKit.Bar(Palette.PoliceBlue, 14f);
            suspicionBar.Value = 0;
            box.AddChild(suspicionBar);

            var cash = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            cash.AddThemeConstantOverride("separation", 12);
            dirtyLabel = UiKit.Label("$ 0 dirty", 13, Palette.DirtyCash);
            cleanLabel = UiKit.Label("$ 0 clean", 13, Palette.CleanCash);
            diamondLabel = UiKit.Label("◆ 0", 13, Palette.Diamond);
            cash.AddChild(dirtyLabel);
            cash.AddChild(cleanLabel);
            cash.AddChild(diamondLabel);
            box.AddChild(cash);
            return panel;
        }

        private Control BuildLogPanel()
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 1.2f, CustomMinimumSize = new Vector2(0f, 150f) };
            log = new RichTextLabel { BbcodeEnabled = true, ScrollFollowing = true, SelectionEnabled = false };
            log.AddThemeFontSizeOverride("normal_font_size", 13);
            panel.AddChild(log);
            return panel;
        }

        private Control BuildActionPanel()
        {
            var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 6);
            panel.AddChild(box);

            box.AddChild(UiKit.Label("AKTION", 12, Palette.TextDim));
            var actions = new HFlowContainer();
            actions.AddThemeConstantOverride("h_separation", 6);
            actions.AddThemeConstantOverride("v_separation", 6);
            attackButton = UiKit.Button("[1] Angriff", "Standard-Attacke");
            skillButton = UiKit.Button("[2] Skill");
            attackButton.Pressed += () => EmitSignal(SignalName.AttackPressed);
            skillButton.Pressed += () => EmitSignal(SignalName.SkillPressed);
            actions.AddChild(attackButton);
            actions.AddChild(skillButton);
            box.AddChild(actions);

            box.AddChild(UiKit.Label("SABOTAGE (1× pro Zug)", 12, new Color(1f, 0.6f, 0.2f)));
            var cheats = new HFlowContainer();
            cheats.AddThemeConstantOverride("h_separation", 6);
            cheats.AddThemeConstantOverride("v_separation", 6);
            brickButton = CheatButton(CheatType.Brick, "[3] Backstein",
                $"{CheatManager.BrickHitChance}% Chance auf Sofort-K.O. des Trainers. +{CheatManager.BrickSuspicion}% Verdacht.");
            dopingButton = CheatButton(CheatType.Doping, $"[4] Doping {CheatManager.DopingCost}$",
                $"+50% Angriff für {CheatManager.DopingTurns} Züge. +{CheatManager.DopingSuspicion}% Verdacht, +{CheatManager.DopingInstability}% Instabilität.");
            bribeButton = CheatButton(CheatType.Bribe, $"[5] Schiri {CheatManager.BribeCost}$",
                $"-{CheatManager.BribeReduction}% Verdacht. {CheatManager.BribeFailChance}% Risiko, dass der Schiri dich verpfeift.");
            cheats.AddChild(brickButton);
            cheats.AddChild(dopingButton);
            cheats.AddChild(bribeButton);
            box.AddChild(cheats);
            return panel;
        }

        private Button CheatButton(CheatType cheat, string text, string tooltip)
        {
            var button = UiKit.Button(text, tooltip);
            button.CustomMinimumSize = new Vector2(120f, 40f);
            button.Pressed += () => EmitSignal(SignalName.CheatPressed, (int)cheat);
            return button;
        }

        private Control BuildResultPanel()
        {
            var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
            center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            resultPanel = new PanelContainer { Visible = false, CustomMinimumSize = new Vector2(380f, 0f) };
            resultPanel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.04f, 0.02f, 0.08f, 0.96f), Palette.NeonPink, 3, 10, 22));
            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 12);
            resultPanel.AddChild(box);
            resultTitle = UiKit.Label("SIEG", 46, Palette.NeonGreen, HorizontalAlignment.Center);
            box.AddChild(resultTitle);
            resultBody = UiKit.Label("", 15, Colors.White, HorizontalAlignment.Center);
            resultBody.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            resultBody.CustomMinimumSize = new Vector2(340f, 0f);
            box.AddChild(resultBody);
            var button = UiKit.Button("Zurück in die Gasse");
            button.Pressed += () => EmitSignal(SignalName.ContinuePressed);
            box.AddChild(button);
            center.AddChild(resultPanel);
            return center;
        }

        private void ApplyResponsiveLayout()
        {
            bool narrow = GetViewportRect().Size.X < NarrowWidth;
            bottomRow.Vertical = narrow;
        }

        private static string EscapeBbCode(string text) => text.Replace("[", "[lb]");
    }
}
