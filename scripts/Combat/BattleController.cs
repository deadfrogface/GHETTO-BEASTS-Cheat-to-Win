using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.Entities;
using GhettoBeasts.Minigames;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;
using Side = GhettoBeasts.Combat.Side;
using MutationSlot = GhettoBeasts.Data.MutationSlot;

namespace GhettoBeasts.Combat
{
    /// <summary>
    /// Root of combat.tscn. Owns the <see cref="BattleEngine"/>, queues its events and replays them
    /// as 3D animations + HUD updates. Contains no combat rules itself.
    /// </summary>
    public partial class BattleController : Node3D
    {
        private const double EventDelay = 0.38;

        private readonly Queue<BattleEvent> pendingEvents = new();
        private readonly List<Node3D> raidUnits = new();

        private CameraRig camera = null!;
        private ArenaRefs arena = null!;
        private BattleHud hud = null!;
        private SetupPanel setup = null!;
        private PrisonEscapeView prison = null!;
        private ShaderMaterial crt = null!;
        private BattleEngine? engine;
        private Beast3D? playerBeast;
        private Beast3D? enemyBeast;
        private Tween? raidTween;
        private bool busy;
        private bool pendingPrison;
        private ColorRect? speedlines;

        private static GameManager Game => GameManager.Instance;

        public override void _Ready()
        {
            if (GameManager.Instance is null)
            {
                GD.PrintErr("[BattleController] GameManager-Autoload fehlt (Projekt → Projekteinstellungen → Autoload).");
                return;
            }

            camera = new CameraRig { Name = "Camera", Fov = CameraRig.CombatFov, Current = true };
            AddChild(camera);

            arena = ArenaBuilder.Build(this);
            if (HasNode("Arena")) VisualManager.EnforceCelShading(GetNode("Arena"));
            camera.PlaceCombatCamera(arena.PlayerSpot, arena.EnemySpot, arena.PlayerTrainer.GlobalPosition);

            var crtLayer = new CanvasLayer { Name = "CrtLayer", Layer = 0 };
            AddChild(crtLayer);
            crt = MaterialFactory.Crt();
            var crtRect = new ColorRect { Material = crt, MouseFilter = Control.MouseFilterEnum.Ignore };
            crtRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            crtLayer.AddChild(crtRect);

            var uiLayer = new CanvasLayer { Name = "UiLayer", Layer = 1 };
            AddChild(uiLayer);
            speedlines = CameraJuice.AttachSpeedlines(uiLayer, new Color(0.85f, 0.9f, 1f, 0.5f));
            hud = new BattleHud { Name = "BattleHud", Visible = false };
            uiLayer.AddChild(hud);
            setup = new SetupPanel { Name = "SetupPanel" };
            uiLayer.AddChild(setup);
            prison = new PrisonEscapeView { Name = "PrisonEscape" };
            uiLayer.AddChild(prison);

            hud.AttackPressed += () => Run(() => PerformAsync(e => e.PlayerAttack()));
            hud.SkillPressed += () => Run(() => PerformAsync(e => e.PlayerSkill()));
            hud.CheatPressed += cheat => Run(() => PerformAsync(e => e.PlayerCheat((CheatType)cheat)));
            hud.ContinuePressed += OnContinuePressed;
            prison.Finished += OnPrisonFinished;
            setup.StartRequested += (cardId, mutationIds, opponentId) => Run(() => StartBattleAsync(cardId, mutationIds, opponentId));
            setup.BackRequested += () => Game.GoTo(string.IsNullOrEmpty(Game.ReturnScenePath) ? GameManager.MainDistrictScene : Game.ReturnScenePath);

            AutoBattleOptions? autoBattle;
            try
            {
                autoBattle = ParseAutoBattleArgs();
            }
            catch (ArgumentException ex)
            {
                GD.PrintErr($"[AutoBattle] Ungültige Argumente: {ex.Message}");
                GetTree().Quit(3);
                return;
            }
            if (autoBattle is not null)
            {
                Run(() => AutoBattleAsync(autoBattle));
                return;
            }

            // Stealth detection / scripted entrance with forced opponent
            if (Game.PendingOpponentId is { Length: > 0 } forcedOpponent)
            {
                string cardId = Game.Save.ActiveFighterId;
                string[] muts = Roster.GetEquippedMutationIds(Game.Save, cardId).ToArray();
                Game.PendingOpponentId = null;
                setup.Visible = false;
                Run(() => StartBattleAsync(cardId, muts, forcedOpponent));
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (engine is null || busy || !hud.Visible) return;
            if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

            Func<BattleEngine, bool>? action = key.Keycode switch
            {
                Key.Key1 => e => e.PlayerAttack(),
                Key.Key2 => e => e.PlayerSkill(),
                Key.Key3 => e => e.PlayerCheat(CheatType.Brick),
                Key.Key4 => e => e.PlayerCheat(CheatType.Doping),
                Key.Key5 => e => e.PlayerCheat(CheatType.Bribe),
                _ => null
            };
            if (key.Keycode == Key.F9) GD.Print(engine.ToDotGraph());
            if (action is null) return;
            GetViewport().SetInputAsHandled();
            Run(() => PerformAsync(action));
        }

        private async void Run(Func<Task> work)
        {
            try
            {
                await work();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[BattleController] {ex}");
                busy = false;
                if (engine is not null) hud.Refresh(engine, false);
            }
        }

        private async Task StartBattleAsync(string cardId, string[] mutationIds, string opponentId)
        {
            if (busy) return;

            Combatant player;
            Combatant enemy;
            try
            {
                if (!Roster.OwnsCard(Game.Save, cardId))
                    Roster.TryAwardCard(Game.Save, cardId); // autobattle / legacy may use unowned ids
                foreach (MutationSlot slot in Enum.GetValues<MutationSlot>())
                    CrisprLab.TryEquip(Game.Save, cardId, slot, null, out _);
                foreach (var mid in mutationIds)
                {
                    if (!Roster.OwnsMutation(Game.Save, mid))
                        Roster.TryAwardMutation(Game.Save, mid);
                    var gene = ContentCatalog.GetMutation(mid);
                    CrisprLab.TryEquip(Game.Save, cardId, gene.Slot, mid, out _);
                }
                Roster.SetActiveFighter(Game.Save, cardId);
                player = CrisprLab.BuildFighter(Game.Save, cardId, Side.Player);
                enemy = Combatant.Create(Side.Enemy, ContentCatalog.GetCard(opponentId));
            }
            catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
            {
                GD.PrintErr($"[BattleController] Ungültiges Setup: {ex.Message}");
                setup.ShowError(ex.Message);
                return;
            }

            Game.Save.LastOpponentId = opponentId;
            Game.Persist();
            Audio.AudioBus.Play("summon");
            Audio.AudioBus.CrossfadeMusic("combat", 0.8);

            ClearBattlefield();
            engine = new BattleEngine(player, enemy, Game.Economy, Game.Random, Game.CombatMods);
            if (engine.DoubleBattleStub)
                hud.LogWarning("2v2 Stub aktiv — Partner-Assist jeder Runde.");
            engine.EventRaised += pendingEvents.Enqueue;
            engine.InvalidAction += OnInvalidAction;
            engine.StateChanged += state => GD.Print($"[BattleEngine] → {state}");

            setup.Visible = false;
            hud.Visible = true;
            hud.Bind(engine);
            hud.SetCurrencies(Game.Economy);

            busy = true;
            hud.DisableActions();
            await hud.ShowBannerAsync("FIGHT!", Palette.NeonCyan, 0.4);
            engine.Start();
            await PlayPendingAsync();
            busy = false;
            hud.Refresh(engine, false);
        }

        private async Task PerformAsync(Func<BattleEngine, bool> action)
        {
            if (engine is null || busy || engine.IsOver) return;
            busy = true;
            hud.DisableActions();
            action(engine);
            await PlayPendingAsync();
            busy = false;
            hud.Refresh(engine, false);
        }

        private void OnInvalidAction(string message)
        {
            GD.PrintErr($"[BattleEngine] {message}");
            hud.LogWarning(message);
        }

        private async Task PlayPendingAsync()
        {
            while (pendingEvents.Count > 0 && engine is not null)
            {
                var battleEvent = pendingEvents.Dequeue();
                await PlayEventAsync(battleEvent);
                hud.Refresh(engine, true);
                if (playerBeast is not null) playerBeast.UpdateStatusGlow(engine.Player);
                if (enemyBeast is not null) enemyBeast.UpdateStatusGlow(engine.Enemy);
            }
        }

        private async Task PlayEventAsync(BattleEvent e)
        {
            hud.Log(e);
            var beast = BeastFor(e.Target);

            switch (e.Type)
            {
                case BattleEventType.Summon when e.Target.HasValue:
                    await SummonAsync(e.Target.Value);
                    break;

                case BattleEventType.Attack when beast is not null:
                    await beast.LungeAsync(OpponentOf(beast)?.GlobalPosition ?? Vector3.Zero);
                    break;

                case BattleEventType.SkillUsed when beast is not null:
                    Vfx.Flash(this, beast.HeadGlobalPosition, Palette.NeonCyan, 6f, 0.5f);
                    _ = hud.ShowBannerAsync(SkillName(e.Target), Palette.NeonCyan, 0.5);
                    await beast.LungeAsync(OpponentOf(beast)?.GlobalPosition ?? Vector3.Zero);
                    break;

                case BattleEventType.Damage when beast is not null:
                    beast.PlayHit(e.Critical);
                    Audio.AudioBus.Play("hit");
                    Vfx.FloatingCombatText(this, beast.HeadGlobalPosition + Vector3.Up * 0.4f, $"-{e.Amount}",
                        e.Critical ? Palette.NeonYellow : Colors.White, e.Critical ? 96 : 68);
                    hud.SetHp(e.Target!.Value, e.ValueAfter);
                    if (e.Critical || e.Amount >= 40) camera.Shake(e.Critical ? 0.28f : 0.14f);
                    if (e.Critical)
                    {
                        camera.Punch(-4f);
                        CameraJuice.HitStop(this, 0.07, 0.2f);
                        CameraJuice.PulseSpeedlines(speedlines, 0.9f, 0.4);
                        CameraJuice.DramaticAction(camera, beast.HeadGlobalPosition, -12f, 0.4);
                    }
                    await Vfx.Delay(this, EventDelay);
                    if (e.ValueAfter <= 0) await beast.FaintAsync();
                    break;

                case BattleEventType.Heal when beast is not null:
                    beast.PlayHeal();
                    Vfx.FloatingText(this, beast.HeadGlobalPosition + Vector3.Up * 0.4f, $"+{e.Amount}", Palette.NeonGreen);
                    hud.SetHp(e.Target!.Value, e.ValueAfter);
                    await Vfx.Delay(this, EventDelay);
                    break;

                case BattleEventType.Miss when beast is not null:
                    Vfx.FloatingText(this, beast.HeadGlobalPosition + Vector3.Up * 0.4f, "MISS", Palette.TextDim, 56);
                    await beast.DodgeAsync();
                    break;

                case BattleEventType.StatusApplied when beast is not null && e.Status.HasValue:
                    Vfx.FloatingText(this, beast.HeadGlobalPosition + Vector3.Up * 0.7f,
                        Palette.StatusName(e.Status.Value).ToUpperInvariant(), Palette.ForStatus(e.Status.Value), 52);
                    await Vfx.Delay(this, EventDelay * 0.8);
                    break;

                case BattleEventType.Instability when e.Target.HasValue:
                    hud.SetInstability(e.Target.Value, e.ValueAfter);
                    if (e.Critical && beast is not null)
                    {
                        PulseGlitch(0.8f, 0.8);
                        camera.Shake(0.3f, 0.6);
                        await beast.WildMutationAsync();
                    }
                    break;

                case BattleEventType.Cheat when e.Cheat.HasValue:
                    await PlayCheatAsync(e.Cheat.Value, e.Critical);
                    break;

                case BattleEventType.Suspicion:
                    hud.SetSuspicion(e.ValueAfter, e.Amount);
                    if (e.Amount > 0) PulseGlitch(Mathf.Clamp(e.ValueAfter / 100f, 0.2f, 0.7f), 0.35);
                    await Vfx.Delay(this, 0.2);
                    break;

                case BattleEventType.Cash:
                    hud.SetCurrencies(Game.Economy);
                    Vfx.FloatingText(this, arena.PlayerTrainer.GlobalPosition + Vector3.Up * 2.6f,
                        e.Amount >= 0 ? $"+{e.Amount} $" : $"{e.Amount} $", e.Amount >= 0 ? Palette.DirtyCash : Palette.Danger, 54);
                    await Vfx.Delay(this, 0.2);
                    break;

                case BattleEventType.Raid:
                    await PlayRaidAsync();
                    break;

                case BattleEventType.Result:
                    await Vfx.Delay(this, 0.5);
                    ShowResult();
                    break;

                default:
                    await Vfx.Delay(this, 0.12);
                    break;
            }
        }

        private async Task SummonAsync(Side side)
        {
            if (engine is null) return;
            var combatant = side == Side.Player ? engine.Player : engine.Enemy;
            var data = CardLibrary.Get(combatant.Definition.Id);
            Vector3 spot = side == Side.Player ? arena.PlayerSpot : arena.EnemySpot;
            Vector3 lookTarget = side == Side.Player ? arena.EnemySpot : arena.PlayerSpot;

            var basis = camera.GlobalTransform.Basis;
            Vector3 hand = side == Side.Player
                ? camera.GlobalPosition - basis.Z * 2.4f - basis.Y * 0.55f - basis.X * 0.75f
                : arena.EnemyTrainer.GlobalPosition + Vector3.Up * 1.9f + (spot - arena.EnemyTrainer.GlobalPosition).Normalized() * 0.6f;

            var card = new Card3D();
            AddChild(card);
            card.Setup(combatant.Definition, data);
            await card.SummonAsync(hand, spot, camera, camera);

            var beast = new Beast3D();
            AddChild(beast);
            beast.GlobalPosition = spot;
            beast.Build(combatant.Definition, data, combatant.Mutations.Values, side);
            beast.LookAt(lookTarget, Vector3.Up, true);
            if (side == Side.Player) playerBeast = beast;
            else enemyBeast = beast;
            await beast.SpawnAsync();
            camera.PlaceCombatCamera(arena.PlayerSpot, arena.EnemySpot, arena.PlayerTrainer.GlobalPosition);
        }

        private async Task PlayCheatAsync(CheatType cheat, bool success)
        {
            Vector3 trainerHand = arena.PlayerTrainer.GlobalPosition + Vector3.Up * 1.6f;
            switch (cheat)
            {
                case CheatType.Brick:
                {
                    Vector3 enemyHead = arena.EnemyTrainer.GlobalPosition + Vector3.Up * 1.75f;
                    Vector3 target = success ? enemyHead : enemyHead + new Vector3(1.4f, -1.2f, -1.5f);
                    await Vfx.ThrowAsync(this, Shapes.Box(0.24f, 0.1f, 0.12f), MaterialFactory.Toon(new Color(0.62f, 0.22f, 0.14f), 0.01f),
                        trainerHand, target, 2.2f, 0.65);
                    Vfx.Burst(this, target, success ? new Color(1f, 0.45f, 0.2f) : new Color(0.6f, 0.5f, 0.45f), success ? 80 : 25, 5f, 0.7f);
                    if (success)
                    {
                        camera.Shake(0.35f, 0.5);
                        Vfx.Flash(this, target, new Color(1f, 0.5f, 0.2f), 10f);
                        Vfx.FloatingText(this, enemyHead + Vector3.Up * 0.6f, "K.O.!", Palette.NeonYellow, 110);
                        var fall = CreateTween();
                        fall.TweenProperty(arena.EnemyTrainer, "rotation_degrees:x", -85f, 0.45)
                            .SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
                        await ToSignal(fall, Tween.SignalName.Finished);
                    }
                    else
                    {
                        Vfx.FloatingText(this, enemyHead + Vector3.Up * 0.6f, "DANEBEN", Palette.TextDim, 64);
                    }
                    break;
                }

                case CheatType.Doping when playerBeast is not null:
                    await Vfx.ThrowAsync(this, Shapes.Cylinder(0.03f, 0.03f, 0.34f), MaterialFactory.Emissive(Palette.NeonGreen, 3f),
                        trainerHand, playerBeast.HeadGlobalPosition, 0.8f, 0.4, 20f);
                    playerBeast.PlayHeal();
                    Vfx.FloatingText(this, playerBeast.HeadGlobalPosition + Vector3.Up * 0.6f, "GEDOPT!", Palette.NeonGreen, 80);
                    camera.Shake(0.1f);
                    await Vfx.Delay(this, 0.3);
                    break;

                case CheatType.Bribe:
                {
                    Vector3 refHand = arena.Referee.GlobalPosition + Vector3.Up * 1.3f;
                    var coin = Shapes.Cylinder(0.07f, 0.07f, 0.015f);
                    var gold = MaterialFactory.Emissive(Palette.NeonYellow, 2.5f);
                    for (int i = 0; i < 4; i++)
                    {
                        _ = Vfx.ThrowAsync(this, coin, gold, trainerHand, refHand, 1.4f, 0.5, 25f);
                        await Vfx.Delay(this, 0.08);
                    }
                    await Vfx.Delay(this, 0.45);
                    if (success)
                    {
                        Vfx.Burst(this, refHand, Palette.NeonYellow, 50, 3f, 0.8f, 0.04f, -3f);
                        Vfx.FloatingText(this, refHand + Vector3.Up * 1f, "$$$", Palette.NeonYellow, 90);
                    }
                    else
                    {
                        Vfx.Flash(this, refHand, Palette.Danger, 8f);
                        Vfx.FloatingText(this, refHand + Vector3.Up * 1f, "VERPFIFFEN!", Palette.Danger, 72);
                        camera.Shake(0.15f);
                    }
                    await Vfx.Delay(this, 0.3);
                    break;
                }
            }
        }

        private async Task PlayRaidAsync()
        {
            hud.DisableActions();
            Audio.AudioBus.Play("raid");
            CameraJuice.HitStop(this, 0.12, 0.25f);
            CameraJuice.PulseSpeedlines(speedlines, 1.1f, 0.7);
            PulseGlitch(1f, 1.4);
            camera.Shake(0.35f, 0.8);

            raidTween?.Kill();
            raidTween = CreateTween().SetLoops();
            var (red, blue) = (arena.RaidLights[0], arena.RaidLights[1]);
            raidTween.TweenProperty(red, "light_energy", 7f, 0.05);
            raidTween.Parallel().TweenProperty(blue, "light_energy", 0f, 0.05);
            raidTween.TweenInterval(0.22);
            raidTween.TweenProperty(red, "light_energy", 0f, 0.05);
            raidTween.Parallel().TweenProperty(blue, "light_energy", 7f, 0.05);
            raidTween.TweenInterval(0.22);

            _ = hud.ShowBannerAsync("SWAT-RAZZIA!", Palette.Danger, 1.4);

            Vector3 target = arena.PlayerTrainer.GlobalPosition;
            Vector3[] entries =
            {
                new(-12f, 0f, 6f), new(-12f, 0f, -2f), new(-6f, 0f, 10f), new(2f, 0f, 10f)
            };
            var tween = CreateTween().SetParallel();
            for (int i = 0; i < entries.Length; i++)
            {
                var unit = new Node3D { Name = $"Swat{i}" };
                AddChild(unit);
                unit.GlobalPosition = entries[i];
                Shapes.Humanoid(unit, new Color(0.08f, 0.1f, 0.18f), new Color(0.1f, 0.1f, 0.12f), Palette.NeonCyan, true);
                Shapes.Part(unit, Shapes.Box(0.5f, 0.7f, 0.05f), new Vector3(0f, 1.1f, 0.35f), MaterialFactory.Toon(new Color(0.15f, 0.2f, 0.3f), 0.01f));
                float angle = Mathf.Tau * i / entries.Length;
                Vector3 stop = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.1f;
                unit.LookAt(stop, Vector3.Up, true);
                tween.TweenProperty(unit, "global_position", stop, 0.9).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
                raidUnits.Add(unit);
            }
            await ToSignal(tween, Tween.SignalName.Finished);

            var cuff = CreateTween();
            cuff.TweenProperty(arena.PlayerTrainer, "rotation_degrees:x", 25f, 0.25);
            await ToSignal(cuff, Tween.SignalName.Finished);
            await Vfx.Delay(this, 0.6);
        }

        private void ShowResult()
        {
            if (engine?.Outcome is not { } outcome) return;
            var save = Game.Save;
            string details;
            switch (outcome)
            {
                case BattleOutcome.Victory:
                    save.Wins++;
                    StreetProgression.RegisterWin(save);
                    details = $"{engine.Enemy.Name} liegt im Dreck.\n+{ContentCatalog.DirtyCashReward(engine.Enemy.Definition.Rarity)} $ Schmutzgeld kassiert.\n{StreetProgression.NextHint(save)}";
                    break;
                case BattleOutcome.Defeat:
                    save.Losses++;
                    details = $"{engine.Player.Name} ist erledigt.\nMehr CRISPR-Gene, mehr Doping … oder ein Backstein?";
                    break;
                default:
                    save.Raids++;
                    pendingPrison = true;
                    details = "Das SWAT-Team hat die Arena gestürmt und dein gesamtes Schmutzgeld beschlagnahmt.\nWeiter → Knast-Tunnel Escape.";
                    break;
            }
            Game.Persist();
            hud.SetCurrencies(Game.Economy);
            hud.ShowResult(outcome, details);
        }

        private void OnContinuePressed()
        {
            if (pendingPrison)
            {
                pendingPrison = false;
                hud.Visible = false;
                ClearBattlefield();
                prison.Begin(Game.Economy, Game.Random);
                return;
            }
            ReturnToSetup();
        }

        private void OnPrisonFinished(int outcome)
        {
            var result = (PrisonOutcome)outcome;
            if (result == PrisonOutcome.Escaped || result == PrisonOutcome.Bribed)
            {
                Game.Economy.Add(Currency.CleanCash, 25);
                GD.Print($"[Prison] Ausbruch ({result}). +25 $ Clean Cash.");
            }
            else
            {
                GD.Print("[Prison] Erwischt. Zurück in die Gasse ohne Bonus.");
            }
            Game.Persist();
            prison.Visible = false;
            ReturnToSetup();
        }

        private void ReturnToSetup()
        {
            ClearBattlefield();
            hud.Visible = false;
            prison.Visible = false;

            // Full loop: leave combat back to hub/district when we entered from there.
            string returnPath = Game.ReturnScenePath;
            if (!string.IsNullOrEmpty(returnPath) &&
                returnPath != GameManager.CombatScene &&
                !OS.GetCmdlineUserArgs().Any(a => a.StartsWith("--autobattle", StringComparison.OrdinalIgnoreCase)))
            {
                Game.GoTo(returnPath);
                return;
            }

            setup.Visible = true;
            setup.Refresh();
        }

        private void ClearBattlefield()
        {
            raidTween?.Kill();
            raidTween = null;
            pendingEvents.Clear();
            if (engine is not null) engine.InvalidAction -= OnInvalidAction;
            engine = null;
            playerBeast?.QueueFree();
            enemyBeast?.QueueFree();
            playerBeast = null;
            enemyBeast = null;
            foreach (var unit in raidUnits) unit.QueueFree();
            raidUnits.Clear();
            arena.ResetActors();
            crt.SetShaderParameter("glitch", 0f);
        }

        private void PulseGlitch(float strength, double duration)
        {
            crt.SetShaderParameter("glitch", strength);
            var tween = CreateTween();
            tween.TweenShaderParameter(crt, "glitch", 0f, duration, from: strength);
        }

        private Beast3D? BeastFor(Side? side) => side switch
        {
            Side.Player => playerBeast,
            Side.Enemy => enemyBeast,
            _ => null
        };

        private Beast3D? OpponentOf(Beast3D beast) => beast.Side == Side.Player ? enemyBeast : playerBeast;

        private string SkillName(Side? side) =>
            engine is null || side is null ? string.Empty
                : (side == Side.Player ? engine.Player : engine.Enemy).Definition.AbilityName.ToUpperInvariant();
    }
}
