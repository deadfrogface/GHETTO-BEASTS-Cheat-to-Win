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

            // Soft dusk wash — not a loud purple slab.
            var wash = new ColorRect
            {
                Color = new Color(0.2f, 0.28f, 0.38f, 0.22f),
                MouseFilter = MouseFilterEnum.Ignore
            };
            wash.SetAnchorsPreset(LayoutPreset.TopWide);
            wash.OffsetBottom = 220;
            AddChild(wash);

            var accentLine = new ColorRect
            {
                Color = Palette.NeonCyan with { A = 0.35f },
                MouseFilter = MouseFilterEnum.Ignore
            };
            accentLine.SetAnchorsPreset(LayoutPreset.TopWide);
            accentLine.OffsetTop = 218;
            accentLine.OffsetBottom = 221;
            AddChild(accentLine);

            var margin = new MarginContainer();
            margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 40);
            margin.AddThemeConstantOverride("margin_right", 40);
            margin.AddThemeConstantOverride("margin_top", 32);
            margin.AddThemeConstantOverride("margin_bottom", 28);
            AddChild(margin);

            var root = new VBoxContainer();
            root.AddThemeConstantOverride("separation", 12);
            margin.AddChild(root);

            root.AddChild(UiKit.Label("GHETTO BEASTS", 44, Colors.White, HorizontalAlignment.Center));
            root.AddChild(UiKit.Label("DNA & Corruption  ·  District Hub", 16, Palette.NeonCyan, HorizontalAlignment.Center));
            var tag = UiKit.Label("Wähle einen Ort — oder geh auf die Straße und schleich dich an den Bullen vorbei.", 13, Palette.TextDim, HorizontalAlignment.Center);
            tag.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            root.AddChild(tag);

            var wallet = new WalletBar();
            root.AddChild(wallet);

            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 10);
            grid.AddThemeConstantOverride("v_separation", 8);
            root.AddChild(grid);

            AddNav(grid, "Gasse betreten", "Isometrische Stealth-Oberwelt mit Cops & Hotspots", Palette.NeonCyan,
                () => Game.GoTo(GameManager.DistrictScene));
            AddNav(grid, "Arena / Käfig", "Direkt in den Rundenkampf (Setup)", Palette.NeonPink,
                () => Game.EnterCombat(GameManager.HubScene));
            AddNav(grid, "CRISPR Lab", "Gene ausrüsten, Stabilisatoren kaufen", Palette.NeonPurple,
                () => Game.GoTo(GameManager.LabScene));
            AddNav(grid, "Waschsalon", "Dirty → Clean Cash (20% Gebühr)", Palette.CleanCash,
                () => Game.GoTo(GameManager.WashScene));
            AddNav(grid, "Darknet Gacha", "Packs für Diamonds öffnen", Palette.Diamond,
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
                12, Palette.TextDim, HorizontalAlignment.Center));

            var footer = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
            footer.AddThemeConstantOverride("separation", 10);
            var reset = UiKit.CompactButton("Spielstand zurücksetzen");
            reset.CustomMinimumSize = new Vector2(200f, 34f);
            reset.Pressed += () =>
            {
                Game.ResetProgress();
                GetTree().ReloadCurrentScene();
            };
            footer.AddChild(reset);
            root.AddChild(footer);

            root.AddChild(UiKit.Label("F5 startet hier. --autobattle springt weiter in den Kampf.", 11, Palette.TextDim, HorizontalAlignment.Center));

            var tween = CreateTween().SetLoops();
            tween.TweenProperty(accentLine, "modulate:a", 0.45f, 1.8).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(accentLine, "modulate:a", 1f, 1.8).SetTrans(Tween.TransitionType.Sine);
        }

        private static void AddNav(GridContainer grid, string title, string tip, Color accent, System.Action action)
        {
            var btn = new Button
            {
                Text = title,
                TooltipText = tip,
                CustomMinimumSize = new Vector2(260f, 52f),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                FocusMode = FocusModeEnum.None
            };
            btn.AddThemeFontSizeOverride("font_size", 15);
            btn.AddThemeStyleboxOverride("normal", UiKit.Box(new Color(0.1f, 0.11f, 0.15f, 0.82f), accent.Darkened(0.35f), 1, 12, 10));
            btn.AddThemeStyleboxOverride("hover", UiKit.Box(new Color(0.14f, 0.16f, 0.22f, 0.9f), accent, 1, 12, 10));
            btn.Pressed += () => action();
            grid.AddChild(btn);
        }
    }
}
