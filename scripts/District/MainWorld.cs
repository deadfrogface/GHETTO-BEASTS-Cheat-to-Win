using System.Linq;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>
    /// Physical Cyberpunk alley hub — primary F5 scene.
    /// Third-person walk, companion follow, E on buildings, ESC Burner Phone, wild encounters.
    /// </summary>
    public partial class MainWorld : Node3D
    {
        private DistrictRefs refs = null!;
        private PlayerController player = null!;
        private CompanionFollow companion = null!;
        private Camera3D cam = null!;
        private BurnerPhone phone = null!;
        private Label prompt = null!;
        private Label status = null!;
        private InteractableBuilding? nearBuilding;
        private bool transitioning;
        private float camYaw;
        private Node3D? swatVan;
        private readonly System.Collections.Generic.List<CopPatrol> cops = new();

        private static GameManager Game => GameManager.Instance;

        public Vector3 PlayerPosition => player?.GlobalPosition ?? Vector3.Zero;
        public Vector3 CompanionPosition => companion?.GlobalPosition ?? Vector3.Zero;
        public bool SwatActive => swatVan is not null || Game.Save.StreetHeat >= 100;

        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[MainWorld] GameManager fehlt.");
                return;
            }

            // Autobattle is handled in GameManager._Ready; if we still land here with the flag, bounce.
            if (WantsAutoBattle())
            {
                Game.EnterCombat(GameManager.MainDistrictScene);
                return;
            }

            refs = DistrictBuilder.Build(this);

            cam = new Camera3D
            {
                Name = "ChaseCam",
                Projection = Camera3D.ProjectionType.Perspective,
                Fov = 48f,
                Current = true
            };
            AddChild(cam);

            player = new PlayerController { Name = "Player" };
            AddChild(player);
            player.GlobalPosition = refs.PlayerSpawn.GlobalPosition;

            companion = new CompanionFollow { Name = "Companion" };
            AddChild(companion);
            companion.Bind(player, Game.Save.ActiveFighterId);

            SpawnCops();
            SpawnWildBeasts();
            if (Game.Save.StreetHeat >= 100)
                SpawnSwatVan(rollIn: false);

            var ui = new CanvasLayer { Name = "Hud", Layer = 1 };
            AddChild(ui);
            BuildHud(ui);

            phone = new BurnerPhone { Name = "BurnerPhone" };
            AddChild(phone);
            phone.BindWorld(this);

            Audio.AudioBus.PlayMusic();
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public void SetWorldPaused(bool paused) => player?.SetInputLocked(paused);

        public void RefreshCompanion() => companion?.Bind(player, Game.Save.ActiveFighterId);

        public override void _Process(double delta)
        {
            if (transitioning || player is null) return;

            // Soft yaw from mouse drag / Q-E
            if (Input.IsMouseButtonPressed(MouseButton.Right) || Input.IsMouseButtonPressed(MouseButton.Middle))
                camYaw -= Input.GetLastMouseVelocity().X * 0.0009f;
            // Q / R rotate camera (E is reserved for building enter)
            if (Input.IsKeyPressed(Key.Q)) camYaw += 1.4f * (float)delta;
            if (Input.IsKeyPressed(Key.R)) camYaw -= 1.4f * (float)delta;

            player.SetCameraYaw(camYaw);

            Vector3 back = Basis.FromEuler(new Vector3(0f, camYaw, 0f)) * Vector3.Back;
            Vector3 target = player.GlobalPosition + back * 7.2f + Vector3.Up * 4.2f;
            cam.GlobalPosition = cam.GlobalPosition.Lerp(target, 1f - Mathf.Exp(-4.5f * (float)delta));
            cam.LookAt(player.GlobalPosition + Vector3.Up * 1.2f, Vector3.Up);

            nearBuilding = null;
            foreach (var b in refs.Buildings)
            {
                if (b.PlayerInside)
                {
                    nearBuilding = b;
                    break;
                }
            }

            // Fallback distance check if Area3D missed CharacterBody
            if (nearBuilding is null)
            {
                float best = 2.0f;
                foreach (var b in refs.Buildings)
                {
                    float d = player.GlobalPosition.DistanceTo(b.GlobalPosition);
                    if (d < best)
                    {
                        best = d;
                        nearBuilding = b;
                    }
                }
                if (best >= 2.0f) nearBuilding = null;
            }

            if (phone.IsOpen)
            {
                prompt.Text = "";
                return;
            }

            prompt.Text = nearBuilding is null
                ? "WASD bewegen · Q/R Kamera · ESC Burner Phone · Wildtiere meiden"
                : $"[E] {nearBuilding.BuildingLabel} betreten";
            prompt.AddThemeColorOverride("font_color",
                nearBuilding is null ? Palette.TextDim : nearBuilding.Accent);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (transitioning || phone.IsOpen || nearBuilding is null) return;
            if (@event is not InputEventKey { Pressed: true, Echo: false, Keycode: Key.E }) return;
            GetViewport().SetInputAsHandled();
            EnterBuilding(nearBuilding);
        }

        private void SpawnCops()
        {
            Vector3[][] paths =
            {
                new[] { new Vector3(-4f, 0f, -6f), new Vector3(2f, 0f, -6f), new Vector3(2f, 0f, -1f), new Vector3(-4f, 0f, -1f) },
                new[] { new Vector3(5f, 0f, 4f), new Vector3(5f, 0f, -3f), new Vector3(8f, 0f, -3f), new Vector3(8f, 0f, 4f) }
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

        private void SpawnSwatVan(bool rollIn = true)
        {
            if (swatVan is not null) return;
            var van = new SwatVan { Name = "SwatVan" };
            AddChild(van);
            swatVan = van;
            Vector3 to = refs.SwatSpawn.GlobalPosition;
            if (rollIn)
            {
                van.RollIn(to + new Vector3(0f, 0f, -14f), to);
                van.Arrived += () =>
                {
                    status.Text = "SWAT-Van vor Ort — Heat 100%!";
                    status.AddThemeColorOverride("font_color", Palette.Danger);
                };
            }
            else
                van.GlobalPosition = to;
            Audio.AudioBus.PlaySpatial(this, "raid", to);
        }

        private void RaiseHeat(int amount)
        {
            int before = Game.Save.StreetHeat;
            Game.Save.StreetHeat = Mathf.Clamp(before + amount, 0, 100);
            if (before < 100 && Game.Save.StreetHeat >= 100)
                SpawnSwatVan(rollIn: true);
            Game.Persist();
        }

        private void SpawnWildBeasts()
        {
            var pool = ContentCatalog.AllCards
                .Where(c => c.Rarity is Rarity.Common or Rarity.Uncommon)
                .Select(c => c.Id)
                .ToList();
            if (pool.Count == 0) pool.Add("kanalratte");

            Vector3[] spots =
            {
                new(-6f, 0f, -4f),
                new(6.5f, 0f, 1f),
                new(-3f, 0f, 7f),
                new(3.5f, 0f, -7f)
            };

            for (int i = 0; i < spots.Length; i++)
            {
                string id = pool[Game.Random.Range(0, pool.Count)];
                var wild = new WildBeast { Name = $"Wild_{id}_{i}" };
                AddChild(wild);
                wild.Setup(id, player, spots[i]);
                wild.EncounterTriggered += OnWildEncounter;
            }
        }

        private void BuildHud(CanvasLayer ui)
        {
            var hudRoot = new Control { Theme = UiKit.CreateTheme() };
            hudRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            ui.AddChild(hudRoot);

            var top = new VBoxContainer();
            top.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
            top.OffsetLeft = 16;
            top.OffsetRight = -16;
            top.OffsetTop = 12;
            top.OffsetBottom = 120;
            hudRoot.AddChild(top);

            top.AddChild(UiKit.Label("MAIN DISTRICT · CYBERPUNK ALLEY", 18, Colors.White));
            top.AddChild(new WalletBar());
            status = UiKit.Label("Begleiter folgt dir. ESC öffnet das Burner Phone.", 12, Palette.TextDim);
            top.AddChild(status);

            prompt = UiKit.Label("", 14, Palette.NeonYellow, HorizontalAlignment.Center);
            prompt.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
            prompt.OffsetTop = -70;
            prompt.OffsetBottom = -30;
            hudRoot.AddChild(prompt);

            var phoneBtn = UiKit.CompactButton("Phone");
            phoneBtn.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
            phoneBtn.OffsetLeft = -120;
            phoneBtn.OffsetTop = 12;
            phoneBtn.OffsetRight = -14;
            phoneBtn.OffsetBottom = 48;
            phoneBtn.Pressed += () => phone.Toggle();
            hudRoot.AddChild(phoneBtn);

            var stick = new VirtualJoystick();
            hudRoot.AddChild(stick);
            stick.StickChanged += v => player.SetJoystick(v);

            prompt.GuiInput += @event =>
            {
                if (nearBuilding is not null && @event is InputEventMouseButton { Pressed: true })
                    EnterBuilding(nearBuilding);
            };
            prompt.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        private void OnSpotted()
        {
            if (transitioning) return;
            RaiseHeat(35);
            transitioning = true;
            status.Text = "ERWISCHT! SWAT zieht dich in den Käfig…";
            status.AddThemeColorOverride("font_color", Palette.Danger);
            Audio.AudioBus.Play("raid");
            if (Game.Save.StreetHeat >= 100)
                SpawnSwatVan(rollIn: true);

            var pool = ContentCatalog.AllCards.Where(c => c.Rarity is Rarity.Common or Rarity.Uncommon or Rarity.Rare).ToList();
            string opponent = pool.Count > 0
                ? pool[Game.Random.Range(0, pool.Count)].Id
                : "kanalratte";

            GetTree().CreateTimer(0.85).Timeout += () =>
                Game.EnterCombat(GameManager.MainDistrictScene, opponent);
        }

        private void OnWildEncounter(string opponentId)
        {
            if (transitioning) return;
            RaiseHeat(12);
            transitioning = true;
            status.Text = $"Wildkontakt: {opponentId}!";
            status.AddThemeColorOverride("font_color", Palette.NeonPink);
            Audio.AudioBus.Play("hit");
            GetTree().CreateTimer(0.35).Timeout += () =>
                Game.EnterCombat(GameManager.MainDistrictScene, opponentId);
        }

        private void EnterBuilding(InteractableBuilding building)
        {
            if (transitioning) return;
            transitioning = true;
            status.Text = $"Betrete {building.BuildingLabel}…";
            if (building.Kind == HotspotKind.Arena)
                Game.EnterCombat(GameManager.MainDistrictScene);
            else
                Game.GoTo(building.ScenePath);
        }

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
    }
}
