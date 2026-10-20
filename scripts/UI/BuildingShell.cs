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
        protected virtual Color Accent => Palette.NeonCyan;

        public override void _Ready()
        {
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();

            var bg = new ColorRect { Color = Palette.Background, MouseFilter = MouseFilterEnum.Ignore };
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
            root.AddThemeConstantOverride("separation", 10);
            margin.AddChild(root);

            var header = new HBoxContainer();
            header.AddThemeConstantOverride("separation", 12);
            root.AddChild(header);
            var titles = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            titles.AddChild(UiKit.Label(Title, 28, Accent));
            var sub = UiKit.Label(Subtitle, 13, Palette.TextDim);
            sub.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            titles.AddChild(sub);
            header.AddChild(titles);
            var back = UiKit.CompactButton("← Gasse", "Zurück in die Main District Alley");
            back.CustomMinimumSize = new Vector2(110f, 34f);
            back.Pressed += () => GameManager.Instance.GoTo(GameManager.MainDistrictScene);
            header.AddChild(back);

            Wallet = new WalletBar();
            root.AddChild(Wallet);

            var panel = new PanelContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
            panel.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.07f, 0.08f, 0.11f, 0.72f), Accent.Darkened(0.4f), 1, 12, 14));
            root.AddChild(panel);

            Body = new VBoxContainer();
            Body.AddThemeConstantOverride("separation", 8);
            panel.AddChild(Body);

            StatusLabel = UiKit.Label("", 12, Palette.NeonCyan);
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
