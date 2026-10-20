using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    /// <summary>Full-screen panel chrome with title, wallet, status and back-to-hub.</summary>
    public abstract partial class BuildingShell : Control
    {
        protected VBoxContainer Body { get; private set; } = null!;
        protected Label StatusLabel { get; private set; } = null!;
        protected WalletBar Wallet { get; private set; } = null!;

        protected abstract string Title { get; }
        protected abstract string Subtitle { get; }
        protected virtual Color Accent => Palette.NeonPink;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();

            var bg = new ColorRect { Color = new Color(0.03f, 0.015f, 0.07f, 1f), MouseFilter = MouseFilterEnum.Ignore };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(bg);

            var margin = new MarginContainer();
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 28);
            margin.AddThemeConstantOverride("margin_right", 28);
            margin.AddThemeConstantOverride("margin_top", 22);
            margin.AddThemeConstantOverride("margin_bottom", 22);
            AddChild(margin);

            var root = new VBoxContainer();
            root.AddThemeConstantOverride("separation", 12);
            margin.AddChild(root);

            var header = new HBoxContainer();
            header.AddThemeConstantOverride("separation", 12);
            root.AddChild(header);
            var titles = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            titles.AddChild(UiKit.Label(Title, 32, Accent));
            var sub = UiKit.Label(Subtitle, 14, Palette.TextDim);
            sub.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            titles.AddChild(sub);
            header.AddChild(titles);
            var back = UiKit.Button("← Hub", "Zurück zum District-Hub");
            back.CustomMinimumSize = new Vector2(140f, 44f);
            back.Pressed += () => GameManager.Instance.GoTo(GameManager.HubScene);
            header.AddChild(back);

            Wallet = new WalletBar();
            root.AddChild(Wallet);

            var panel = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.04f, 0.02f, 0.09f, 0.92f), Accent.Darkened(0.25f), 2, 10, 16));
            root.AddChild(panel);

            Body = new VBoxContainer();
            Body.AddThemeConstantOverride("separation", 10);
            panel.AddChild(Body);

            StatusLabel = UiKit.Label("", 13, Palette.NeonCyan);
            StatusLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            root.AddChild(StatusLabel);

            BuildContent();
            SetStatus("Bereit.");
            Wallet.Refresh();
        }

        protected abstract void BuildContent();

        protected void SetStatus(string text, bool error = false)
        {
            StatusLabel.Text = text;
            StatusLabel.AddThemeColorOverride("font_color", error ? Palette.Danger : Palette.NeonCyan);
        }

        protected void Persist()
        {
            GameManager.Instance.Persist();
            Wallet.Refresh();
        }
    }
}
