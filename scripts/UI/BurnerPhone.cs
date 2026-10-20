using System.Linq;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.District;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.UI
{
    /// <summary>
    /// Diegetic jailbroken smartphone overlay (ESC / TAB / ui_cancel).
    /// Homescreen apps are meta-only; buildings stay physical world entries.
    /// Pause pattern: CanvasLayer + ProcessMode WhenPaused (Godot docs).
    /// </summary>
    public partial class BurnerPhone : CanvasLayer
    {
        public bool IsOpen { get; private set; }

        private Control root = null!;
        private Control phoneFrame = null!;
        private Control homescreen = null!;
        private Control appScreen = null!;
        private Label statusBar = null!;
        private VBoxContainer appBody = null!;
        private Label appTitle = null!;
        private MainWorld? world;
        private ScrollContainer appScroll = null!;

        private static GameManager Game => GameManager.Instance;

        public void BindWorld(MainWorld mainWorld) => world = mainWorld;

        public override void _Ready()
        {
            Layer = 80;
            ProcessMode = ProcessModeEnum.Always;
            Visible = false;

            root = new Control { Theme = UiKit.CreateTheme() };
            root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            AddChild(root);

            var dim = new ColorRect
            {
                Color = new Color(0.02f, 0.03f, 0.05f, 0.55f),
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            root.AddChild(dim);

            phoneFrame = new PanelContainer();
            phoneFrame.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            phoneFrame.CustomMinimumSize = new Vector2(340, 600);
            phoneFrame.AddThemeStyleboxOverride("panel", UiKit.Box(
                new Color(0.08f, 0.09f, 0.12f, 0.97f),
                Palette.NeonCyan.Darkened(0.25f), 2, 28, 14));
            root.AddChild(phoneFrame);
            phoneFrame.OffsetLeft = -170;
            phoneFrame.OffsetTop = -300;
            phoneFrame.OffsetRight = 170;
            phoneFrame.OffsetBottom = 300;

            var col = new VBoxContainer();
            col.AddThemeConstantOverride("separation", 8);
            phoneFrame.AddChild(col);

            statusBar = UiKit.Label("BURNER · JAILBREAK 13.37  ·  ▮▮▮▮", 11, Palette.NeonGreen, HorizontalAlignment.Center);
            col.AddChild(statusBar);

            homescreen = new VBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
            homescreen.AddThemeConstantOverride("separation", 8);
            col.AddChild(homescreen);

            homescreen.AddChild(UiKit.Label("GHETTO OS", 22, Colors.White, HorizontalAlignment.Center));
            homescreen.AddChild(UiKit.Label("Meta-Apps. Gebäude zu Fuß betreten.", 11, Palette.TextDim, HorizontalAlignment.Center));

            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 8);
            grid.AddThemeConstantOverride("v_separation", 8);
            homescreen.AddChild(grid);

            AddApp(grid, "Käfig", "3D + CRISPR", Palette.NeonPink, OpenCage);
            AddApp(grid, "Darknet", "Pack Flip", Palette.Diamond, OpenDarknet);
            AddApp(grid, "GPS", "Icons", Palette.NeonCyan, OpenGps);
            AddApp(grid, "Wallet", "Laundry Timer", Palette.DirtyCash, OpenWallet);
            AddApp(grid, "Contacts", "Bribes", Palette.NeonYellow, OpenContacts);
            AddApp(grid, "Settings", "Audio / Toon", Palette.TextDim, OpenSettings);

            var saveBtn = UiKit.Button("Save & Sync");
            saveBtn.Pressed += () =>
            {
                Game.Persist();
                statusBar.Text = "BURNER · SAVE OK  ·  ▮▮▮▮";
                Audio.AudioBus.Play("ui");
            };
            homescreen.AddChild(saveBtn);

            var closeBtn = UiKit.CompactButton("Phone weg (ESC)");
            closeBtn.Pressed += Close;
            homescreen.AddChild(closeBtn);

            appScreen = new VBoxContainer { Visible = false, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
            appScreen.AddThemeConstantOverride("separation", 6);
            col.AddChild(appScreen);

            var appHeader = new HBoxContainer();
            appScreen.AddChild(appHeader);
            var back = UiKit.CompactButton("←");
            back.CustomMinimumSize = new Vector2(44, 32);
            back.Pressed += ShowHome;
            appHeader.AddChild(back);
            appTitle = UiKit.Label("", 16, Palette.NeonCyan);
            appTitle.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            appHeader.AddChild(appTitle);

            appScroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
            };
            appScreen.AddChild(appScroll);
            appBody = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            appBody.AddThemeConstantOverride("separation", 6);
            appScroll.AddChild(appBody);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventKey { Pressed: true, Echo: false } key)
            {
                if (key.Keycode is Key.Escape or Key.Tab)
                {
                    Toggle();
                    GetViewport().SetInputAsHandled();
                }
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                Toggle();
                GetViewport().SetInputAsHandled();
            }
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Visible = true;
            ShowHome();
            GetTree().Paused = true;
            ProcessMode = ProcessModeEnum.WhenPaused;
            world?.SetWorldPaused(true);
            statusBar.Text = $"BURNER · {Game.Save.ActiveFighterId}  ·  ▮▮▮▮";
            Audio.AudioBus.Play("ui");
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Visible = false;
            GetTree().Paused = false;
            ProcessMode = ProcessModeEnum.Always;
            world?.SetWorldPaused(false);
        }

        private void ShowHome()
        {
            homescreen.Visible = true;
            appScreen.Visible = false;
        }

        private void OpenApp(string title)
        {
            homescreen.Visible = false;
            appScreen.Visible = true;
            appTitle.Text = title;
            foreach (var child in appBody.GetChildren())
                child.QueueFree();
        }

        private void OpenCage()
        {
            OpenApp("Käfig · Roster");
            appBody.AddChild(UiKit.Label("3D-Preview drehen · Stats · CRISPR-Slots", 11, Palette.TextDim));

            var host = new SubViewportContainer
            {
                Stretch = true,
                CustomMinimumSize = new Vector2(0f, 160f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            appBody.AddChild(host);
            var vp = new SubViewport
            {
                Size = new Vector2I(420, 200),
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always
            };
            host.AddChild(vp);
            var world3 = new Node3D();
            vp.AddChild(world3);
            world3.AddChild(new Camera3D { Position = new Vector3(0f, 1.1f, 2.6f), Current = true, Fov = 38f });
            world3.AddChild(new OmniLight3D { Position = new Vector3(1.2f, 2f, 1.5f), LightColor = Palette.NeonPink, LightEnergy = 2.8f });
            var previewRoot = new Node3D { Name = "Preview" };
            world3.AddChild(previewRoot);
            BuildCagePreview(previewRoot, Game.Save.ActiveFighterId);

            var spin = CreateTween().SetLoops();
            spin.TweenProperty(previewRoot, "rotation_degrees:y", 360f, 4.5);

            foreach (var card in Roster.OwnedCards(Game.Save))
            {
                bool active = card.Id == Game.Save.ActiveFighterId;
                var loadout = Roster.GetEquippedMutationIds(Game.Save, card.Id);
                var row = new VBoxContainer();
                appBody.AddChild(row);
                row.AddChild(UiKit.Label(
                    $"{card.Name} [{card.Rarity.DisplayName()}]",
                    13, active ? Palette.NeonYellow : Colors.White));
                row.AddChild(UiKit.Label(
                    $"HP {card.BaseStats.MaxHp}  ATK {card.BaseStats.Attack}  DEF {card.BaseStats.Defense}  ·  Gene {loadout.Count}/3  ·  Inst {Roster.PreviewInstability(Game.Save, card.Id)}%",
                    10, Palette.TextDim));

                var actions = new HBoxContainer();
                actions.AddThemeConstantOverride("separation", 4);
                row.AddChild(actions);

                var pick = UiKit.CompactButton(active ? "Aktiv" : "Wählen");
                pick.Disabled = active;
                string id = card.Id;
                pick.Pressed += () =>
                {
                    Roster.SetActiveFighter(Game.Save, id);
                    Game.Persist();
                    world?.RefreshCompanion();
                    OpenCage();
                };
                actions.AddChild(pick);

                var crispr = UiKit.CompactButton("CRISPR");
                crispr.Pressed += () =>
                {
                    Close();
                    Game.GoTo(GameManager.LabScene);
                };
                actions.AddChild(crispr);
            }
        }

        private static void BuildCagePreview(Node3D root, string cardId)
        {
            foreach (var c in root.GetChildren()) c.QueueFree();
            if (!ContentCatalog.TryGetCard(cardId, out var def))
            {
                Shapes.Toon(root, Shapes.Sphere(0.45f), new Vector3(0f, 0.45f, 0f), Palette.NeonCyan);
                return;
            }
            var data = CardLibrary.Get(cardId);
            var muts = Roster.ResolveMutations(GameManager.Instance.Save, cardId);
            var beast = new Entities.Beast3D();
            root.AddChild(beast);
            beast.Build(def, data, muts, Combat.Side.Player);
            if (beast.GetNodeOrNull<Node3D>("Model") is { } inner)
                inner.Scale = Vector3.One * Mathf.Max(0.12f, data.ModelScale) * 0.75f;
        }

        private void OpenDarknet()
        {
            OpenApp("Darknet · Pack Flip");
            appBody.AddChild(UiKit.Label(
                $"1 ◆ · inkl. Gen-Corrupted ({DarknetGacha.Weights.Last().Weight:0.#}%)",
                11, Palette.TextDim));

            var host = new SubViewportContainer
            {
                Stretch = true,
                CustomMinimumSize = new Vector2(0f, 140f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            appBody.AddChild(host);
            var vp = new SubViewport
            {
                Size = new Vector2I(400, 180),
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always
            };
            host.AddChild(vp);
            var w3 = new Node3D();
            vp.AddChild(w3);
            w3.AddChild(new Camera3D { Position = new Vector3(0f, 1f, 2.8f), Current = true, Fov = 40f });
            w3.AddChild(new OmniLight3D { Position = new Vector3(1f, 2f, 2f), LightEnergy = 3f, LightColor = Palette.Diamond });
            var packRoot = new Node3D();
            w3.AddChild(packRoot);
            BuildPackMesh(packRoot, Palette.NeonPink);

            var result = UiKit.Label("Noch kein Pull.", 12, Palette.NeonCyan);
            result.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            appBody.AddChild(result);

            var glow = new ColorRect
            {
                CustomMinimumSize = new Vector2(0f, 8f),
                Color = new Color(0f, 0f, 0f, 0f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            appBody.AddChild(glow);

            var pull = UiKit.Button($"Pack flip (−{DarknetGacha.PackCostDiamonds} ◆)");
            pull.Pressed += async () =>
            {
                var res = DarknetGacha.OpenPack(Game.Save, Game.Economy, Game.Random);
                if (!res.Success)
                {
                    result.Text = res.Message;
                    result.AddThemeColorOverride("font_color", Palette.Danger);
                    return;
                }
                Game.Persist();
                Audio.AudioBus.Play("pack");
                var rarity = res.Pull!.Rarity;
                Color glowColor = Palette.ForRarity(rarity);
                glow.Color = glowColor;
                BuildPackMesh(packRoot, glowColor);
                var tween = CreateTween();
                tween.TweenProperty(packRoot, "rotation_degrees:y", 720f, 0.65)
                    .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                tween.Parallel().TweenProperty(packRoot, "scale", new Vector3(1.35f, 1.35f, 1.35f), 0.35);
                tween.TweenProperty(packRoot, "scale", Vector3.One, 0.2);
                await ToSignal(tween, Tween.SignalName.Finished);
                packRoot.RotationDegrees = Vector3.Zero;
                string tag = rarity == Rarity.GenCorrupted ? "⚠ GEN-CORRUPTED" : rarity.DisplayName();
                result.Text = $"{res.Pull.DisplayName}\n{tag}\n{res.Message}";
                result.AddThemeColorOverride("font_color", glowColor);
                statusBar.Text = $"◆ {Game.Economy.Diamonds}  ·  Roster {Game.Save.OwnedCardIds.Count}";
                if (rarity == Rarity.GenCorrupted)
                    Visuals.CameraJuice.HitStop(this, 0.08, 0.35f);
            };
            appBody.AddChild(pull);
        }

        private static void BuildPackMesh(Node3D packRoot, Color accent)
        {
            foreach (var child in packRoot.GetChildren()) child.QueueFree();
            packRoot.AddChild(new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(1.0f, 1.3f, 0.22f) },
                MaterialOverride = MaterialFactory.Toon(new Color(0.12f, 0.05f, 0.22f), 0.03f)
            });
            packRoot.AddChild(new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(1.05f, 0.14f, 0.25f) },
                Position = new Vector3(0f, 0.32f, 0f),
                MaterialOverride = MaterialFactory.Emissive(accent, 2.8f)
            });
            VisualManager.EnforceCelShading(packRoot);
        }

        private void OpenGps()
        {
            OpenApp("GPS · District");
            appBody.AddChild(UiKit.Label("Icons: du · Gebäude · Heat", 11, Palette.TextDim));

            var map = new PanelContainer { CustomMinimumSize = new Vector2(0, 240) };
            map.AddThemeStyleboxOverride("panel", UiKit.Box(new Color(0.05f, 0.07f, 0.1f), Palette.NeonCyan.Darkened(0.4f), 1, 8, 8));
            appBody.AddChild(map);

            var draw = new MinimapDraw { World = world };
            draw.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            draw.CustomMinimumSize = new Vector2(280, 220);
            map.AddChild(draw);

            appBody.AddChild(UiKit.Label("● Arena  ◆ CRISPR  ○ Wash  ▲ Dope  ■ Weap  ★ Darknet", 10, Palette.TextDim));
            appBody.AddChild(UiKit.Label($"Street Heat: {Game.Save.StreetHeat}%", 12,
                Game.Save.StreetHeat >= 100 ? Palette.Danger : Palette.NeonYellow));
        }

        private void OpenWallet()
        {
            OpenApp("Wallet · Laundry");
            var e = Game.Economy;
            appBody.AddChild(UiKit.Label($"Dirty  $ {e.DirtyCash}", 16, Palette.DirtyCash));
            appBody.AddChild(UiKit.Label($"Clean  $ {e.CleanCash}", 16, Palette.CleanCash));
            appBody.AddChild(UiKit.Label($"Diamonds  ◆ {e.Diamonds}", 16, Palette.Diamond));
            appBody.AddChild(UiKit.Label(
                $"Bilanz  {Game.Save.Wins}W / {Game.Save.Losses}L / {Game.Save.Raids} Raids",
                11, Palette.TextDim));

            var job = Game.Save.Laundry ?? new LaundryJob();
            Game.Save.Laundry = job;
            if (job.Active)
            {
                int left = job.SecondsRemaining();
                appBody.AddChild(UiKit.Label(
                    left > 0 ? $"Waschgang: {job.DirtyAmount} $ · noch {left}s" : $"Waschgang fertig: {job.DirtyAmount} $ Dirty",
                    12, left > 0 ? Palette.NeonYellow : Palette.NeonGreen));
                if (left <= 0)
                {
                    var claim = UiKit.Button("Clean abholen");
                    claim.Pressed += () =>
                    {
                        if (LaundryService.TryClaim(Game.Save, Game.Economy, out var msg))
                        {
                            Game.Persist();
                            OpenWallet();
                        }
                        statusBar.Text = msg;
                    };
                    appBody.AddChild(claim);
                }
            }
            else
            {
                appBody.AddChild(UiKit.Label($"Timer-Wäsche ({LaundryService.DefaultDurationSeconds}s, −{Economy.LaunderFee:P0} Fee)", 11, Palette.TextDim));
                foreach (int amount in new[] { 50, 100, 200 })
                {
                    int capture = amount;
                    var btn = UiKit.CompactButton($"Wash {capture} $");
                    btn.Pressed += () =>
                    {
                        if (LaundryService.TryStart(Game.Save, Game.Economy, capture, out var msg))
                        {
                            Game.Persist();
                            OpenWallet();
                        }
                        statusBar.Text = msg;
                        statusBar.AddThemeColorOverride("font_color", Palette.NeonCyan);
                    };
                    appBody.AddChild(btn);
                }
            }

            var gotoWash = UiKit.CompactButton("Zur Wash-Bude (3D)");
            gotoWash.Pressed += () =>
            {
                Close();
                Game.GoTo(GameManager.WashScene);
            };
            appBody.AddChild(gotoWash);
        }

        private void OpenContacts()
        {
            OpenApp("Contacts · Bribes");
            appBody.AddChild(UiKit.Label("Freischalten → Kampf-Boni", 11, Palette.TextDim));

            appBody.AddChild(UiKit.Label(
                Game.Save.LawyerUnlocked
                    ? "✔ Anwalt (−15% SWAT-Verdacht)"
                    : $"Anwalt · {ContactsService.LawyerCostClean} $ Clean → −15% Verdacht",
                12, Game.Save.LawyerUnlocked ? Palette.NeonGreen : Colors.White));
            if (!Game.Save.LawyerUnlocked)
            {
                var buy = UiKit.Button("Anwalt kaufen");
                buy.Pressed += () =>
                {
                    if (ContactsService.TryUnlockLawyer(Game.Save, Game.Economy, out var msg))
                        Game.Persist();
                    statusBar.Text = msg;
                    OpenContacts();
                };
                appBody.AddChild(buy);
            }

            appBody.AddChild(UiKit.Label(
                Game.Save.RefUnlocked
                    ? $"✔ Schiri-Flatrate ({ContactsService.RefFlatrateClean} $ Clean / Bestechung)"
                    : $"Schiri · {ContactsService.RefCostClean} $ Clean → Flatrate",
                12, Game.Save.RefUnlocked ? Palette.NeonGreen : Colors.White));
            if (!Game.Save.RefUnlocked)
            {
                var buy = UiKit.Button("Schiri-Nummer kaufen");
                buy.Pressed += () =>
                {
                    if (ContactsService.TryUnlockRef(Game.Save, Game.Economy, out var msg))
                        Game.Persist();
                    statusBar.Text = msg;
                    OpenContacts();
                };
                appBody.AddChild(buy);
            }
        }

        private void OpenSettings()
        {
            OpenApp("Settings");
            var p = Game.Save.Phone ?? new PhoneSettings();
            Game.Save.Phone = p;

            void Slider(string label, float value, System.Action<float> apply)
            {
                appBody.AddChild(UiKit.Label(label, 11, Palette.TextDim));
                var s = new HSlider
                {
                    MinValue = 0,
                    MaxValue = 1,
                    Step = 0.05,
                    Value = value,
                    CustomMinimumSize = new Vector2(0f, 28f),
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                s.ValueChanged += v =>
                {
                    apply((float)v);
                    Game.ApplyPhoneSettings();
                    Game.Persist();
                };
                appBody.AddChild(s);
            }

            Slider($"Master {p.MasterVolume:0.00}", p.MasterVolume, v => p.MasterVolume = v);
            Slider($"Music {p.MusicVolume:0.00}", p.MusicVolume, v => p.MusicVolume = v);
            Slider($"SFX {p.SfxVolume:0.00}", p.SfxVolume, v => p.SfxVolume = v);

            var cel = new CheckButton { Text = "Cel-Shading", ButtonPressed = p.CelShading };
            cel.Toggled += on => { p.CelShading = on; Game.Persist(); };
            appBody.AddChild(cel);

            var outline = new CheckButton { Text = "Outline", ButtonPressed = p.Outline };
            outline.Toggled += on => { p.Outline = on; Game.Persist(); };
            appBody.AddChild(outline);

            appBody.AddChild(UiKit.Label($"FPS Cap: {p.TargetFps}", 11, Palette.TextDim));
            var fps = new HSlider
            {
                MinValue = 30,
                MaxValue = 120,
                Step = 15,
                Value = p.TargetFps,
                CustomMinimumSize = new Vector2(0f, 28f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            fps.ValueChanged += v =>
            {
                p.TargetFps = (int)v;
                Game.ApplyPhoneSettings();
                Game.Persist();
                OpenSettings();
            };
            appBody.AddChild(fps);

            var cloud = new CheckButton { Text = "Cloud Sync (Stub)", ButtonPressed = p.CloudSyncStub };
            cloud.Toggled += on =>
            {
                p.CloudSyncStub = on;
                statusBar.Text = on ? "Cloud: queued (stub)" : "Cloud: off";
                Game.Persist();
            };
            appBody.AddChild(cloud);

            var reset = UiKit.Button("Save Reset", "Löscht lokalen Spielstand");
            reset.Pressed += () =>
            {
                Game.ResetProgress();
                statusBar.Text = "SAVE RESET";
                statusBar.AddThemeColorOverride("font_color", Palette.Danger);
                OpenSettings();
            };
            appBody.AddChild(reset);
        }

        private static void AddApp(GridContainer grid, string title, string tip, Color accent, System.Action action)
        {
            var btn = new Button
            {
                Text = $"{title}\n{tip}",
                CustomMinimumSize = new Vector2(140, 68),
                FocusMode = Control.FocusModeEnum.None
            };
            btn.AddThemeFontSizeOverride("font_size", 12);
            btn.AddThemeStyleboxOverride("normal", UiKit.Box(new Color(0.1f, 0.11f, 0.15f, 0.9f), accent.Darkened(0.3f), 1, 14, 8));
            btn.AddThemeStyleboxOverride("hover", UiKit.Box(new Color(0.14f, 0.16f, 0.2f, 0.95f), accent, 1, 14, 8));
            btn.Pressed += () => action();
            grid.AddChild(btn);
        }

        private partial class MinimapDraw : Control
        {
            public MainWorld? World { get; set; }

            public override void _Draw()
            {
                var rect = GetRect();
                DrawRect(rect, new Color(0.06f, 0.08f, 0.12f));
                Vector2 Map(Vector3 w)
                {
                    float u = Mathf.InverseLerp(-11f, 11f, w.X);
                    float v = Mathf.InverseLerp(-12f, 12f, w.Z);
                    return new Vector2(rect.Position.X + u * rect.Size.X, rect.Position.Y + v * rect.Size.Y);
                }

                void Spot(Vector3 pos, Color c, float r = 4f) => DrawCircle(Map(pos), r, c);

                Spot(new Vector3(0f, 0f, -10f), Palette.NeonPink, 5f);   // Arena
                Spot(new Vector3(-9f, 0f, -2f), Palette.NeonPurple, 5f); // CRISPR
                Spot(new Vector3(9f, 0f, -2f), Palette.CleanCash, 5f);   // Wash
                Spot(new Vector3(-9f, 0f, 6f), Palette.NeonGreen, 5f);   // Dope
                Spot(new Vector3(9f, 0f, 6f), Palette.Danger, 5f);       // Weap
                Spot(new Vector3(0f, 0f, 10f), Palette.Diamond, 5f);     // Darknet

                if (World?.PlayerPosition is { } pp)
                    Spot(pp, Palette.NeonYellow, 6f);
                if (World?.CompanionPosition is { } cp)
                    Spot(cp, Palette.NeonCyan, 3.5f);
                if (World?.SwatActive == true)
                    Spot(new Vector3(0f, 0f, -8f), Palette.PoliceBlue, 7f);
            }

            public override void _Process(double delta) => QueueRedraw();
        }
    }
}
