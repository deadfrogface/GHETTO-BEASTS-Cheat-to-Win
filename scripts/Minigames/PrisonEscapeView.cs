using System;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Minigames
{
    /// <summary>Grid-based tunnel dig UI shown after a SWAT raid.</summary>
    public partial class PrisonEscapeView : Control
    {
        [Signal] public delegate void FinishedEventHandler(int outcome);

        private PrisonEscapeEngine? engine;
        private GridContainer grid = null!;
        private Label statusLabel = null!;
        private RichTextLabel log = null!;
        private Button bribeButton = null!;
        private bool finishedEmitted;
        private readonly System.Collections.Generic.Dictionary<(int, int), Panel> cells = new();

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();
            Visible = false;

            var dim = new ColorRect { Color = new Color(0.01f, 0.02f, 0.04f, 0.92f), MouseFilter = MouseFilterEnum.Ignore };
            dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(dim);

            var center = new CenterContainer();
            center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(center);

            var panel = new PanelContainer { CustomMinimumSize = new Vector2(720f, 0f) };
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.03f, 0.05f, 0.1f, 0.96f), Palette.PoliceBlue, 2, 10, 16));
            center.AddChild(panel);

            var box = new VBoxContainer();
            box.AddThemeConstantOverride("separation", 8);
            panel.AddChild(box);

            box.AddChild(UiKit.Label("KNAST-TUNNEL", 32, Palette.PoliceBlue.Lightened(0.3f), HorizontalAlignment.Center));
            box.AddChild(UiKit.Label("Grabe dich zur EXIT-Zelle. Pfeiltasten / WASD. Werkzeuge knacken Fels.", 13, Palette.TextDim, HorizontalAlignment.Center));

            statusLabel = UiKit.Label("", 14, Colors.White, HorizontalAlignment.Center);
            box.AddChild(statusLabel);

            grid = new GridContainer { Columns = PrisonEscapeEngine.DefaultWidth };
            grid.AddThemeConstantOverride("h_separation", 3);
            grid.AddThemeConstantOverride("v_separation", 3);
            var gridCenter = new CenterContainer();
            gridCenter.AddChild(grid);
            box.AddChild(gridCenter);

            var controls = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            controls.AddThemeConstantOverride("separation", 6);
            controls.AddChild(DirButton("↑", DigDirection.Up));
            controls.AddChild(DirButton("←", DigDirection.Left));
            controls.AddChild(DirButton("↓", DigDirection.Down));
            controls.AddChild(DirButton("→", DigDirection.Right));
            bribeButton = UiKit.Button($"Wärter schmieren ({PrisonEscapeEngine.BribeDiamondCost} ◆)");
            bribeButton.Pressed += () => engine?.TryBribeGuard();
            controls.AddChild(bribeButton);
            box.AddChild(controls);

            log = new RichTextLabel { BbcodeEnabled = true, CustomMinimumSize = new Vector2(0f, 90f), ScrollFollowing = true };
            log.AddThemeFontSizeOverride("normal_font_size", 13);
            box.AddChild(log);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!Visible || engine is null || engine.IsOver) return;
            if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

            DigDirection? dir = key.Keycode switch
            {
                Key.Up or Key.W => DigDirection.Up,
                Key.Down or Key.S => DigDirection.Down,
                Key.Left or Key.A => DigDirection.Left,
                Key.Right or Key.D => DigDirection.Right,
                _ => null
            };
            if (dir is null) return;
            GetViewport().SetInputAsHandled();
            engine.TryMove(dir.Value);
        }

        public void Begin(Economy economy, IRandom random)
        {
            if (engine is not null)
            {
                engine.Log -= OnLog;
                engine.Changed -= Refresh;
            }

            engine = new PrisonEscapeEngine(economy, random);
            engine.Log += OnLog;
            engine.Changed += Refresh;
            finishedEmitted = false;
            log.Clear();
            Visible = true;
            BuildGrid();
            Refresh();
        }

        private Button DirButton(string text, DigDirection direction)
        {
            var button = UiKit.Button(text);
            button.CustomMinimumSize = new Vector2(56f, 44f);
            button.Pressed += () => engine?.TryMove(direction);
            return button;
        }

        private void BuildGrid()
        {
            foreach (var child in grid.GetChildren()) child.QueueFree();
            cells.Clear();
            if (engine is null) return;
            grid.Columns = engine.Width;
            foreach (var (x, y) in engine.AllCells())
            {
                var cell = new Panel { CustomMinimumSize = new Vector2(42f, 42f) };
                var label = UiKit.Label("", 14, Colors.White, HorizontalAlignment.Center);
                label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                cell.AddChild(label);
                grid.AddChild(cell);
                cells[(x, y)] = cell;
            }
        }

        private void Refresh()
        {
            if (engine is null) return;
            statusLabel.Text = $"Lärm {engine.Noise}%   ·   Werkzeuge {engine.Tools}   ·   Züge {engine.Moves}   ·   ◆ {GameManager.Instance.Economy.Diamonds}";
            bribeButton.Disabled = engine.IsOver || GameManager.Instance.Economy.Diamonds < PrisonEscapeEngine.BribeDiamondCost;

            foreach (var (pos, panel) in cells)
            {
                var (x, y) = pos;
                bool revealed = engine.IsRevealed(x, y);
                bool isPlayer = x == engine.PlayerX && y == engine.PlayerY;
                var tile = engine.TileAt(x, y);
                var label = panel.GetChild<Label>(0);

                Color bg;
                string text;
                if (!revealed)
                {
                    bg = new Color(0.05f, 0.06f, 0.1f);
                    text = "?";
                }
                else if (isPlayer)
                {
                    bg = Palette.NeonCyan;
                    text = "@";
                }
                else
                {
                    (bg, text) = tile switch
                    {
                        PrisonTile.Rock => (new Color(0.25f, 0.22f, 0.28f), "#"),
                        PrisonTile.Tunnel or PrisonTile.Start => (new Color(0.15f, 0.12f, 0.08f), "."),
                        PrisonTile.Exit => (Palette.NeonGreen, "E"),
                        PrisonTile.Guard => (Palette.Danger, "W"),
                        PrisonTile.Tool => (Palette.NeonYellow, "T"),
                        PrisonTile.Vent => (Palette.NeonPurple, "V"),
                        _ => (new Color(0.2f, 0.16f, 0.12f), ",")
                    };
                }

                panel.AddThemeStyleboxOverride("panel", UiKit.Box(bg, bg.Lightened(0.2f), 1, 4, 0));
                label.Text = text;
                label.AddThemeColorOverride("font_color", isPlayer || tile is PrisonTile.Exit or PrisonTile.Guard ? Colors.Black : Colors.White);
            }

            if (engine.IsOver && !finishedEmitted)
            {
                finishedEmitted = true;
                CallDeferred(MethodName.EmitFinished, (int)engine.Outcome);
            }
        }

        private void EmitFinished(int outcome) => EmitSignal(SignalName.Finished, outcome);

        private void OnLog(string message) =>
            log.AppendText($"[color=#{UiKit.Hex(Palette.TextDim)}]{message}[/color]\n");
    }
}
