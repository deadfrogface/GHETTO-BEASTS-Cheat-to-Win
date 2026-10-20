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
        private Label questHud = null!;
        private InteractableBuilding? nearBuilding;
        private bool transitioning;
        private float camYaw;
        private Node3D? swatVan;
        private readonly System.Collections.Generic.List<CopPatrol> cops = new();

        private static GameManager Game => GameManager.Instance;

        public Vector3 PlayerPosition => player?.GlobalPosition ?? Vector3.Zero;
        public Vector3 CompanionPosition => companion?.GlobalPosition ?? Vector3.Zero;
        public bool SwatActive => swatVan is not null || Game.Save.StreetHeat >= 100;
        public DistrictRefs Refs => refs;

        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[MainWorld] GameManager fehlt.");
                return;
            }

            if (WantsAutoBattle())
            {
                Game.EnterCombat(GameManager.MainDistrictScene);
                return;
            }

            StreetProgression.EnsureDefaults(Game.Save);
            refs = DistrictBuilder.Build(this, Game.Save);

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
            player.SetPlayBounds(refs.PlayBounds);

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

            Audio.AudioBus.PlayMusic("district");
            SetProcess(true);
            SetPhysicsProcess(true);
        }

        public void SetWorldPaused(bool paused) => player?.SetInputLocked(paused);

        public void RefreshCompanion() => companion?.Bind(player, Game.Save.ActiveFighterId);

        public override void _Process(double delta)
        {
            if (transitioning || player is null) return;

            if (Input.IsMouseButtonPressed(MouseButton.Right) || Input.IsMouseButtonPressed(MouseButton.Middle))
                camYaw -= Input.GetLastMouseVelocity().X * 0.0009f;
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

            if (nearBuilding is null)
            {
                prompt.Text = "WASD/Stick · LOOK-Pad · Q/R Kamera · ESC Phone · Wildtiere meiden";
                prompt.AddThemeColorOverride("font_color", Palette.TextDim);
            }
            else if (!nearBuilding.IsUnlocked)
            {
                prompt.Text = $"[LOCKED] {nearBuilding.BuildingLabel} — {StreetProgression.NextHint(Game.Save)}";
                prompt.AddThemeColorOverride("font_color", Palette.Danger);
            }
            else
            {
                prompt.Text = $"[E] {nearBuilding.BuildingLabel} betreten";
                prompt.AddThemeColorOverride("font_color", nearBuilding.Accent);
            }

            questHud.Text = StreetProgression.NextHint(Game.Save);
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
            foreach (var pathStart in refs.CopSpawns)
            {
                Vector3[] path =
                {
                    pathStart,
                    pathStart + new Vector3(3f, 0f, 0f),
                    pathStart + new Vector3(3f, 0f, 3f),
                    pathStart + new Vector3(0f, 0f, 3f)
                };
                var cop = new CopPatrol { Name = $"Cop_{cops.Count}" };
                AddChild(cop);
                cop.Setup(path, player);
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
            var beastPool = ContentCatalog.AllCards
                .Where(c => c.Kind == CardKind.Beast && c.Rarity is Rarity.Common or Rarity.Uncommon)
                .Select(c => c.Id)
                .ToList();
            if (beastPool.Count == 0) beastPool.Add("kanalratte");

            var trainerPool = ContentCatalog.AllCards
                .Where(c => c.Kind == CardKind.Npc && c.Rarity is Rarity.Common or Rarity.Uncommon or Rarity.Rare)
                .Select(c => c.Id)
                .ToList();
            if (trainerPool.Count == 0) trainerPool.Add("chantal");

            for (int i = 0; i < refs.WildSpots.Count; i++)
            {
                string id = beastPool[Game.Random.Range(0, beastPool.Count)];
                var wild = new WildBeast { Name = $"Wild_{id}_{i}" };
                AddChild(wild);
                wild.Setup(id, player, refs.WildSpots[i]);
                wild.EncounterTriggered += OnWildEncounter;
            }

            for (int i = 0; i < refs.TrainerSpots.Count; i++)
            {
                string id = trainerPool[Game.Random.Range(0, trainerPool.Count)];
                var trainer = new WildBeast { Name = $"Trainer_{id}_{i}", AggroRadius = 1.75f, WanderSpeed = 1.1f };
                AddChild(trainer);
                trainer.Setup(id, player, refs.TrainerSpots[i], trainer: true);
                trainer.EncounterTriggered += OnWildEncounter;
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
            top.OffsetBottom = 140;
            hudRoot.AddChild(top);

            string districtTitle = "MAIN DISTRICT";
            if (refs.BackAlleyOpen) districtTitle += " + BACK ALLEY";
            if (refs.DockBlockOpen) districtTitle += " + DOCK";
            top.AddChild(UiKit.Label(districtTitle, 18, Colors.White));
            top.AddChild(new WalletBar());
            status = UiKit.Label("Begleiter folgt dir. ESC öffnet das Burner Phone.", 12, Palette.TextDim);
            top.AddChild(status);
            questHud = UiKit.Label(StreetProgression.NextHint(Game.Save), 12, Palette.NeonYellow);
            questHud.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            top.AddChild(questHud);

            prompt = UiKit.Label("", 14, Palette.NeonYellow, HorizontalAlignment.Center);
            prompt.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
            prompt.OffsetTop = -78;
            prompt.OffsetBottom = -28;
            hudRoot.AddChild(prompt);

            var phoneBtn = UiKit.Button("PHONE");
            phoneBtn.CustomMinimumSize = new Vector2(120, 56);
            phoneBtn.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
            phoneBtn.OffsetLeft = -140;
            phoneBtn.OffsetTop = 12;
            phoneBtn.OffsetRight = -14;
            phoneBtn.OffsetBottom = 72;
            phoneBtn.Pressed += () =>
            {
                Audio.AudioBus.Play("ui");
                phone.Toggle();
            };
            hudRoot.AddChild(phoneBtn);

            var stick = new VirtualJoystick();
            hudRoot.AddChild(stick);
            stick.StickChanged += v => player.SetJoystick(v);

            var look = new TouchLookPad();
            hudRoot.AddChild(look);
            look.LookDelta += d => camYaw -= d.X * 0.008f;

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
            Audio.AudioBus.CrossfadeMusic("combat", 0.6);
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
            Audio.AudioBus.CrossfadeMusic("combat", 0.5);
            GetTree().CreateTimer(0.35).Timeout += () =>
                Game.EnterCombat(GameManager.MainDistrictScene, opponentId);
        }

        private void EnterBuilding(InteractableBuilding building)
        {
            if (transitioning) return;
            if (!building.IsUnlocked)
            {
                status.Text = StreetProgression.NextHint(Game.Save);
                status.AddThemeColorOverride("font_color", Palette.Danger);
                Audio.AudioBus.Play("ui");
                return;
            }
            transitioning = true;
            status.Text = $"Betrete {building.BuildingLabel}…";
            Audio.AudioBus.Play("ui");
            if (building.Kind == HotspotKind.Arena)
            {
                Audio.AudioBus.CrossfadeMusic("combat", 0.7);
                Game.EnterCombat(GameManager.MainDistrictScene);
            }
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
