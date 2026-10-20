using System.Linq;
using GhettoBeasts.Core;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Hub
{
    /// <summary>District hub / main menu — navigate to alley, arena and buildings.</summary>
    public partial class HubController : Control
    {
        private static GameManager Game => GameManager.Instance;

        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[Hub] GameManager-Autoload fehlt.");
                return;
            }

            // Headless / CI: --autobattle is redirected by GameManager; --autohub just stays here briefly.
            foreach (var arg in OS.GetCmdlineUserArgs())
            {
                if (arg.Equals("--autohub", System.StringComparison.OrdinalIgnoreCase))
                {
                    GD.Print("[Hub] --autohub smoke OK");
                    GetTree().CreateTimer(0.4).Timeout += () => GetTree().Quit(0);
                }
            }

            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Theme = UiKit.CreateTheme();
            BuildUi();
        }

        private void BuildUi()
        {
            var bg = new ColorRect { Color = Palette.Background, MouseFilter = MouseFilterEnum.Ignore };
            bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            AddChild(bg);

            // Atmospheric gradient strips
            var glow = new ColorRect
            {
                Color = new Color(0.45f, 0.05f, 0.35f, 0.35f),
                MouseFilter = MouseFilterEnum.Ignore,
                Position = new Vector2(0, 0),
                Size = new Vector2(1280, 180)
            };
            glow.SetAnchorsPreset(LayoutPreset.TopWide);
            glow.OffsetBottom = 180;
            AddChild(glow);

            var margin = new MarginContainer();
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 36);
            margin.AddThemeConstantOverride("margin_right", 36);
            margin.AddThemeConstantOverride("margin_top", 28);
            margin.AddThemeConstantOverride("margin_bottom", 28);
            AddChild(margin);

            var root = new VBoxContainer();
            root.AddThemeConstantOverride("separation", 14);
            margin.AddChild(root);

            root.AddChild(UiKit.Label("GHETTO BEASTS", 48, Palette.NeonPink, HorizontalAlignment.Center));
            root.AddChild(UiKit.Label("DNA & Corruption  ·  District Hub", 18, Palette.NeonCyan, HorizontalAlignment.Center));
            var tag = UiKit.Label("Wähle einen Ort — oder geh auf die Straße und schleich dich an den Bullen vorbei.", 14, Palette.TextDim, HorizontalAlignment.Center);
            tag.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            root.AddChild(tag);

            var wallet = new WalletBar();
            root.AddChild(wallet);

            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 12);
            grid.AddThemeConstantOverride("v_separation", 10);
            root.AddChild(grid);

            AddNav(grid, "Gasse betreten", "Isometrische Stealth-Oberwelt mit Cops & Hotspots", Palette.NeonCyan,
                () => Game.GoTo(GameManager.DistrictScene));
            AddNav(grid, "Arena / Kaefig", "Direkt in den Rundenkampf (Setup)", Palette.NeonPink,
                () => Game.EnterCombat(GameManager.HubScene));
            AddNav(grid, "CRISPR Lab", "Gene ausruesten, Stabilisatoren kaufen", Palette.NeonPurple,
                () => Game.GoTo(GameManager.LabScene));
            AddNav(grid, "Waschsalon", "Dirty → Clean Cash (20% Gebuehr)", Palette.CleanCash,
                () => Game.GoTo(GameManager.WashScene));
            AddNav(grid, "Darknet Gacha", "Packs fuer Diamonds oeffnen", Palette.Diamond,
                () => Game.GoTo(GameManager.GachaScene));
            AddNav(grid, "DOPE Shop", "Schwarzmarkt-Chemie (Stub)", Palette.NeonGreen,
                () => Game.GoTo(GameManager.DopeScene));
            AddNav(grid, "WEAP Shop", "Schlagringe & Billig-Taser (Stub)", Palette.Danger,
                () => Game.GoTo(GameManager.WeapScene));
            AddNav(grid, "Speichern", "Spielstand jetzt schreiben", Palette.NeonYellow, () =>
            {
                Game.Persist();
                wallet.Refresh();
                GD.Print("[Hub] Spielstand gespeichert.");
            });

            var fighter = Game.Save.ActiveFighterId;
            root.AddChild(UiKit.Label(
                $"Aktiver Kämpfer: {fighter}  ·  Gene: {string.Join(", ", Roster.GetEquippedMutationIds(Game.Save, fighter).DefaultIfEmpty("—"))}",
                13, Palette.TextDim, HorizontalAlignment.Center));

            var footer = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            footer.AddThemeConstantOverride("separation", 10);
            var reset = UiKit.Button("Spielstand zurücksetzen");
            reset.Pressed += () =>
            {
                Game.ResetProgress();
                GetTree().ReloadCurrentScene();
            };
            footer.AddChild(reset);
            root.AddChild(footer);

            root.AddChild(UiKit.Label("F5 startet hier. --autobattle springt weiter in den Kampf.", 12, Palette.TextDim, HorizontalAlignment.Center));

            // Subtle title pulse
            var tween = CreateTween().SetLoops();
            tween.TweenProperty(glow, "modulate:a", 0.55f, 1.6).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(glow, "modulate:a", 1f, 1.6).SetTrans(Tween.TransitionType.Sine);
        }

        private static void AddNav(GridContainer grid, string title, string tip, Color accent, System.Action action)
        {
            var btn = new Button
            {
                Text = title,
                TooltipText = tip,
                CustomMinimumSize = new Vector2(280f, 64f),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                FocusMode = FocusModeEnum.None
            };
            btn.AddThemeFontSizeOverride("font_size", 17);
            btn.AddThemeStyleboxOverride("normal", UiKit.Box(new Color(0.1f, 0.04f, 0.18f, 0.95f), accent.Darkened(0.2f), 2, 8, 12));
            btn.AddThemeStyleboxOverride("hover", UiKit.Box(new Color(0.18f, 0.06f, 0.28f, 0.98f), accent, 2, 8, 12));
            btn.Pressed += () => action();
            grid.AddChild(btn);
        }
    }
}
