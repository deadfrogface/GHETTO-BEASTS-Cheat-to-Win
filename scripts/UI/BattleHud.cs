using System.Threading.Tasks;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;
using Side = GhettoBeasts.Combat.Side;

namespace GhettoBeasts.UI
{
    /// <summary>Minimal anime-style battle HUD: edge nameplates + compact action cluster.</summary>
    public partial class BattleHud : Control
    {
        [Signal] public delegate void AttackPressedEventHandler();
        [Signal] public delegate void SkillPressedEventHandler();
        [Signal] public delegate void CheatPressedEventHandler(int cheat);
        [Signal] public delegate void ContinuePressedEventHandler();

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

            // Player nameplate — top-left edge
            playerPanel = new CombatantPanel { MouseFilter = MouseFilterEnum.Ignore };
            playerPanel.SetAnchorsPreset(LayoutPreset.TopLeft);
            playerPanel.OffsetLeft = 14;
            playerPanel.OffsetTop = 12;
            playerPanel.OffsetRight = 270;
            playerPanel.OffsetBottom = 110;
            AddChild(playerPanel);

            // Enemy nameplate — top-right edge
            enemyPanel = new CombatantPanel { MouseFilter = MouseFilterEnum.Ignore };
            enemyPanel.SetAnchorsPreset(LayoutPreset.TopRight);
            enemyPanel.GrowHorizontal = GrowDirection.Begin;
            enemyPanel.OffsetLeft = -270;
            enemyPanel.OffsetTop = 12;
            enemyPanel.OffsetRight = -14;
            enemyPanel.OffsetBottom = 110;
            AddChild(enemyPanel);

            AddChild(BuildTopStrip());
            AddChild(BuildLogPanel());
            AddChild(BuildActionCluster());

            banner = UiKit.Label("", 42, Palette.NeonCyan, HorizontalAlignment.Center);
            banner.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
            banner.GrowHorizontal = GrowDirection.Both;
            banner.GrowVertical = GrowDirection.Both;
            banner.AddThemeConstantOverride("outline_size", 10);
            banner.Visible = false;
            banner.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(banner);

            AddChild(BuildResultPanel());
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
            roundLabel.Text = engine.IsOver ? "ENDE" : $"R{engine.Round}";
            playerPanel.SetStatuses(engine.Player);
            enemyPanel.SetStatuses(engine.Enemy);
            playerPanel.SetInstability(engine.Player.Instability);
            enemyPanel.SetInstability(engine.Enemy.Instability);

            bool canAct = !busy && engine.IsPlayersTurn;
            attackButton.Disabled = !(canAct && engine.CanAttack);
            skillButton.Disabled = !(canAct && engine.CanUseSkill);
            skillButton.Text = engine.Player.SkillReady
                ? $"[2] {engine.Player.Definition.AbilityName}"
                : $"[2] CD {engine.Player.SkillCooldown}";
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
            var color = value >= 80 ? Palette.Danger : value >= 50 ? new Color(0.9f, 0.55f, 0.25f) : Palette.PoliceBlue.Lightened(0.2f);
            UiKit.SetBarColor(suspicionBar, color);
            suspicionLabel.Text = $"SWAT {value}%";
            suspicionLabel.AddThemeColorOverride("font_color", color);

            if (delta == 0) return;
            suspicionTween?.Kill();
            suspicionBar.Scale = Vector2.One;
            suspicionBar.PivotOffset = suspicionBar.Size / 2f;
            suspicionTween = CreateTween();
            suspicionTween.TweenProperty(suspicionBar, "scale", new Vector2(1.04f, 1.35f), 0.08);
            suspicionTween.TweenProperty(suspicionBar, "scale", Vector2.One, 0.22).SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        }

        public void SetCurrencies(Economy economy)
        {
            dirtyLabel.Text = $"${economy.DirtyCash}";
            cleanLabel.Text = $"€{economy.CleanCash}";
            diamondLabel.Text = $"◆{economy.Diamonds}";
        }

        public void Log(BattleEvent battleEvent)
        {
            var color = battleEvent.Type switch
            {
                BattleEventType.Damage => battleEvent.Critical ? Palette.NeonYellow : battleEvent.Target == Side.Player ? new Color(0.95f, 0.55f, 0.58f) : Colors.White,
                BattleEventType.Heal => Palette.NeonGreen,
                BattleEventType.Miss => Palette.TextDim,
                BattleEventType.StatusApplied => battleEvent.Status.HasValue ? Palette.ForStatus(battleEvent.Status.Value) : Colors.White,
                BattleEventType.SkillUsed => Palette.NeonCyan,
                BattleEventType.Instability => Palette.NeonPurple,
                BattleEventType.Cheat => new Color(0.92f, 0.62f, 0.32f),
                BattleEventType.Suspicion => Palette.PoliceBlue.Lightened(0.25f),
                BattleEventType.Cash => Palette.DirtyCash,
                BattleEventType.Raid => Palette.Danger,
                BattleEventType.Result => Palette.NeonPink,
                BattleEventType.Summon => Palette.NeonCyan,
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
            banner.Scale = Vector2.One * 1.6f;
            banner.Modulate = Colors.White with { A = 0f };

            var tween = CreateTween();
            tween.SetParallel();
            tween.TweenProperty(banner, "scale", Vector2.One, 0.22).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(banner, "modulate:a", 1f, 0.12);
            tween.Chain().TweenInterval(holdSeconds);
            tween.Chain().TweenProperty(banner, "modulate:a", 0f, 0.2);
            await ToSignal(tween, Tween.SignalName.Finished);
            banner.Visible = false;
        }

        public void ShowResult(BattleOutcome outcome, string details)
        {
            (string title, Color color) = outcome switch
            {
                BattleOutcome.Victory => ("SIEG", Palette.NeonGreen),
                BattleOutcome.Defeat => ("NIEDERLAGE", Palette.Danger),
                _ => ("BUSTED", Palette.PoliceBlue.Lightened(0.25f))
            };
            resultTitle.Text = title;
            resultTitle.AddThemeColorOverride("font_color", color);
            resultBody.Text = details;
            resultPanel.Visible = true;
            resultPanel.Modulate = Colors.White with { A = 0f };
            CreateTween().TweenProperty(resultPanel, "modulate:a", 1f, 0.25);
            DisableActions();
        }

        public void HideResult() => resultPanel.Visible = false;

        private CombatantPanel Panel(Side side) => side == Side.Player ? playerPanel : enemyPanel;

        private Control BuildTopStrip()
        {
            var strip = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore };
            strip.SetAnchorsPreset(LayoutPreset.TopWide);
            strip.OffsetLeft = 290;
            strip.OffsetRight = -290;
            strip.OffsetTop = 10;
            strip.OffsetBottom = 52;
            strip.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.06f, 0.07f, 0.1f, 0.45f), Palette.PanelBorder, 1, 8, 6));

            var box = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            box.AddThemeConstantOverride("separation", 14);
            strip.AddChild(box);

            roundLabel = UiKit.Label("R1", 12, Palette.TextDim, HorizontalAlignment.Center);
            box.AddChild(roundLabel);
            suspicionLabel = UiKit.Label("SWAT 0%", 12, Palette.PoliceBlue, HorizontalAlignment.Center);
            box.AddChild(suspicionLabel);
            suspicionBar = UiKit.Bar(Palette.PoliceBlue, 7f);
            suspicionBar.CustomMinimumSize = new Vector2(120f, 7f);
            suspicionBar.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            suspicionBar.Value = 0;
            box.AddChild(suspicionBar);

            dirtyLabel = UiKit.Label("$0", 12, Palette.DirtyCash);
            cleanLabel = UiKit.Label("€0", 12, Palette.CleanCash);
            diamondLabel = UiKit.Label("◆0", 12, Palette.Diamond);
            box.AddChild(dirtyLabel);
            box.AddChild(cleanLabel);
            box.AddChild(diamondLabel);
            return strip;
        }

        private Control BuildLogPanel()
        {
            var panel = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore };
            panel.SetAnchorsPreset(LayoutPreset.BottomLeft);
            panel.OffsetLeft = 12;
            panel.OffsetTop = -148;
            panel.OffsetRight = 320;
            panel.OffsetBottom = -12;
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.05f, 0.06f, 0.09f, 0.42f), Palette.PanelBorder, 1, 8, 6));
            log = new RichTextLabel
            {
                BbcodeEnabled = true,
                ScrollFollowing = true,
                SelectionEnabled = false,
                MouseFilter = MouseFilterEnum.Ignore
            };
            log.AddThemeFontSizeOverride("normal_font_size", 11);
            if (UiKit.Font is { } font) log.AddThemeFontOverride("normal_font", font);
            panel.AddChild(log);
            return panel;
        }

        private Control BuildActionCluster()
        {
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(LayoutPreset.BottomRight);
            panel.GrowHorizontal = GrowDirection.Begin;
            panel.GrowVertical = GrowDirection.Begin;
            panel.OffsetLeft = -280;
            panel.OffsetTop = -210;
            panel.OffsetRight = -12;
            panel.OffsetBottom = -12;
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.06f, 0.07f, 0.1f, 0.55f), Palette.PanelBorder, 1, 14, 10));

            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 5);
            panel.AddChild(box);

            box.AddChild(UiKit.Label("AKTIONEN", 10, Palette.TextDim));
            var actions = new GridContainer { Columns = 2 };
            actions.AddThemeConstantOverride("h_separation", 5);
            actions.AddThemeConstantOverride("v_separation", 5);
            attackButton = UiKit.CompactButton("[1] Angriff", "Standard-Attacke");
            skillButton = UiKit.CompactButton("[2] Skill");
            attackButton.Pressed += () => EmitSignal(SignalName.AttackPressed);
            skillButton.Pressed += () => EmitSignal(SignalName.SkillPressed);
            actions.AddChild(attackButton);
            actions.AddChild(skillButton);
            box.AddChild(actions);

            box.AddChild(UiKit.Label("SABOTAGE", 10, new Color(0.9f, 0.6f, 0.35f)));
            var cheats = new VBoxContainer();
            cheats.AddThemeConstantOverride("separation", 4);
            brickButton = CheatButton(CheatType.Brick, "[3] Backstein",
                $"{CheatManager.BrickHitChance}% Sofort-K.O. · +{CheatManager.BrickSuspicion}% Verdacht");
            dopingButton = CheatButton(CheatType.Doping, $"[4] Doping {CheatManager.DopingCost}$",
                $"+50% ATK · +{CheatManager.DopingSuspicion}% Verdacht");
            bribeButton = CheatButton(CheatType.Bribe, $"[5] Schiri {CheatManager.BribeCost}$",
                $"-{CheatManager.BribeReduction}% Verdacht · Risiko {CheatManager.BribeFailChance}%");
            cheats.AddChild(brickButton);
            cheats.AddChild(dopingButton);
            cheats.AddChild(bribeButton);
            box.AddChild(cheats);
            return panel;
        }

        private Button CheatButton(CheatType cheat, string text, string tooltip)
        {
            var button = UiKit.CompactButton(text, tooltip);
            button.CustomMinimumSize = new Vector2(240f, 32f);
            button.Pressed += () => EmitSignal(SignalName.CheatPressed, (int)cheat);
            return button;
        }

        private Control BuildResultPanel()
        {
            var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
            center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            resultPanel = new PanelContainer { Visible = false, CustomMinimumSize = new Vector2(340f, 0f) };
            resultPanel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.06f, 0.07f, 0.1f, 0.92f), Palette.NeonCyan.Darkened(0.25f), 1, 12, 18));
            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 10);
            resultPanel.AddChild(box);
            resultTitle = UiKit.Label("SIEG", 36, Palette.NeonGreen, HorizontalAlignment.Center);
            box.AddChild(resultTitle);
            resultBody = UiKit.Label("", 14, Colors.White, HorizontalAlignment.Center);
            resultBody.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            resultBody.CustomMinimumSize = new Vector2(300f, 0f);
            box.AddChild(resultBody);
            var button = UiKit.CompactButton("Weiter");
            button.CustomMinimumSize = new Vector2(160f, 36f);
            button.Pressed += () => EmitSignal(SignalName.ContinuePressed);
            box.AddChild(button);
            center.AddChild(resultPanel);
            return center;
        }

        private static string EscapeBbCode(string text) => text.Replace("[", "[lb]");
    }
}
