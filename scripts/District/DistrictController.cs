using System.Linq;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Stealth alley loop: move, avoid cop cones, enter buildings via hotspots.</summary>
    public partial class DistrictController : Node3D
    {
        private DistrictRefs refs = null!;
        private PlayerController player = null!;
        private Label prompt = null!;
        private Label status = null!;
        private HotspotDef? nearHotspot;
        private bool transitioning;
        private readonly System.Collections.Generic.List<CopPatrol> cops = new();

        private static GameManager Game => GameManager.Instance;

        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[District] GameManager fehlt.");
                return;
            }

            refs = DistrictBuilder.Build(this);

            var cam = new Camera3D
            {
                Name = "IsoCam",
                Projection = Camera3D.ProjectionType.Perspective,
                Fov = 42f,
                Current = true,
                Position = new Vector3(0f, 16f, 14f)
            };
            AddChild(cam);
            cam.LookAt(new Vector3(0f, 0f, 0f));

            player = new PlayerController { Name = "Player" };
            AddChild(player);
            player.GlobalPosition = refs.PlayerSpawn.GlobalPosition;

            SpawnCops();

            var ui = new CanvasLayer { Name = "Ui", Layer = 1 };
            AddChild(ui);
            BuildHud(ui);

            // Follow camera lightly
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            if (GetNodeOrNull<Camera3D>("IsoCam") is { } cam)
            {
                Vector3 target = player.GlobalPosition + new Vector3(0f, 16f, 14f);
                cam.GlobalPosition = cam.GlobalPosition.Lerp(target, 1f - Mathf.Exp(-3f * (float)delta));
                cam.LookAt(player.GlobalPosition, Vector3.Up);
            }

            nearHotspot = null;
            float best = 2.2f;
            foreach (var hs in refs.Hotspots)
            {
                float d = player.GlobalPosition.DistanceTo(hs.Position);
                if (d < best)
                {
                    best = d;
                    nearHotspot = hs;
                }
            }
            prompt.Text = nearHotspot is null
                ? "WASD / Stick bewegen · Cops meiden"
                : $"[E / Tippen] {nearHotspot.Label} betreten";
            prompt.AddThemeColorOverride("font_color", nearHotspot is null ? Palette.TextDim : nearHotspot.Color);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (transitioning || nearHotspot is null) return;
            if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.E }) return;
            GetViewport().SetInputAsHandled();
            EnterHotspot(nearHotspot);
        }

        private void SpawnCops()
        {
            Vector3[][] paths =
            {
                new[] { new Vector3(-4f, 0f, -6f), new Vector3(2f, 0f, -6f), new Vector3(2f, 0f, -1f), new Vector3(-4f, 0f, -1f) },
                new[] { new Vector3(5f, 0f, 4f), new Vector3(5f, 0f, -3f), new Vector3(8f, 0f, -3f), new Vector3(8f, 0f, 4f) },
                new[] { new Vector3(-2f, 0f, 8f), new Vector3(-6f, 0f, 8f), new Vector3(-6f, 0f, 3f), new Vector3(-2f, 0f, 3f) }
            };

            for (int i = 0; i < paths.Length; i++)
            {
                var cop = new CopPatrol { Name = $"Cop{i}" };
                AddChild(cop);
                cop.Setup(paths[i], player);
                cop.PlayerSpotted += OnSpotted;
                cops.Add(cop);
            }
        }

        private void BuildHud(CanvasLayer ui)
        {
            var root = new Control { Theme = UiKit.CreateTheme() };
            root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            ui.AddChild(root);

            var top = new VBoxContainer();
            top.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
            top.OffsetLeft = 16;
            top.OffsetRight = -16;
            top.OffsetTop = 12;
            top.OffsetBottom = 120;
            root.AddChild(top);

            top.AddChild(UiKit.Label("DISTRICT · STEALTH", 18, Colors.White));
            top.AddChild(new WalletBar());
            status = UiKit.Label("Bleib außerhalb der blauen Sichtkegel.", 12, Palette.TextDim);
            top.AddChild(status);

            prompt = UiKit.Label("", 14, Palette.NeonYellow, HorizontalAlignment.Center);
            prompt.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
            prompt.OffsetTop = -70;
            prompt.OffsetBottom = -30;
            root.AddChild(prompt);

            var hubBtn = UiKit.CompactButton("← Hub");
            hubBtn.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
            hubBtn.OffsetLeft = -130;
            hubBtn.OffsetTop = 12;
            hubBtn.OffsetRight = -14;
            hubBtn.OffsetBottom = 48;
            hubBtn.Pressed += () => Game.GoTo(GameManager.HubScene);
            root.AddChild(hubBtn);

            var stick = new VirtualJoystick();
            root.AddChild(stick);
            stick.StickChanged += v => player.SetJoystick(v);

            // Tap prompt area to enter hotspot on mobile
            prompt.GuiInput += @event =>
            {
                if (nearHotspot is not null && @event is InputEventMouseButton { Pressed: true })
                    EnterHotspot(nearHotspot);
            };
            prompt.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        private void OnSpotted()
        {
            if (transitioning) return;
            transitioning = true;
            status.Text = "ERWISCHT! SWAT zieht dich in den Käfig…";
            status.AddThemeColorOverride("font_color", Palette.Danger);
            Audio.AudioBus.Play("raid");

            // Random street opponent
            var pool = ContentCatalog.AllCards.Where(c => c.Rarity is Rarity.Common or Rarity.Uncommon or Rarity.Rare).ToList();
            string opponent = pool.Count > 0
                ? pool[Game.Random.Range(0, pool.Count)].Id
                : "kanalratte";

            GetTree().CreateTimer(0.85).Timeout += () =>
                Game.EnterCombat(GameManager.DistrictScene, opponent);
        }

        private void EnterHotspot(HotspotDef hs)
        {
            if (transitioning) return;
            transitioning = true;
            status.Text = $"Betrete {hs.Label}…";
            if (hs.Kind == HotspotKind.Arena)
                Game.EnterCombat(GameManager.DistrictScene);
            else
                Game.GoTo(hs.ScenePath);
        }
    }
}
