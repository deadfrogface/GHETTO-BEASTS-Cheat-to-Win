using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public partial class DarknetView : BuildingShell
    {
        private RichTextLabel resultLabel = null!;
        private Node3D packRoot = null!;
        private SubViewport viewport = null!;

        protected override string Title => "DARKNET GACHA";
        protected override string Subtitle => $"Packs für {DarknetGacha.PackCostDiamonds} ◆. Gewichtete Seltenheit — Karten oder CRISPR-Gene.";
        protected override Color Accent => Palette.Diamond;

        protected override void BuildContent()
        {
            Body.AddChild(UiKit.Label(
                "Common 45% · Uncommon 28% · Rare 16% · Epic 8% · Legendary 3%  |  30% Chance auf Gen statt Karte",
                13, Palette.TextDim));

            var host = new SubViewportContainer
            {
                Stretch = true,
                CustomMinimumSize = new Vector2(0f, 180f),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            Body.AddChild(host);
            viewport = new SubViewport
            {
                Size = new Vector2I(640, 220),
                TransparentBg = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always
            };
            host.AddChild(viewport);

            var world = new Node3D();
            viewport.AddChild(world);
            world.AddChild(new Camera3D { Position = new Vector3(0f, 1.2f, 3.2f), Current = true, Fov = 40f });
            world.AddChild(new OmniLight3D { Position = new Vector3(1f, 2f, 2f), LightColor = Palette.NeonCyan, LightEnergy = 3f });
            packRoot = new Node3D { Name = "Pack" };
            world.AddChild(packRoot);
            BuildPackMesh();

            var open = UiKit.Button($"PACK ÖFFNEN (−{DarknetGacha.PackCostDiamonds} ◆)");
            open.CustomMinimumSize = new Vector2(0f, 54f);
            open.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            open.Pressed += OnOpen;
            Body.AddChild(open);

            resultLabel = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false };
            Body.AddChild(resultLabel);
            resultLabel.Text = "[color=#aaa]Noch kein Pack geöffnet.[/color]";
        }

        private void BuildPackMesh()
        {
            foreach (var child in packRoot.GetChildren()) child.QueueFree();
            var box = new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(1.1f, 1.4f, 0.25f) },
                MaterialOverride = MaterialFactory.Toon(new Color(0.12f, 0.05f, 0.22f), 0.03f)
            };
            packRoot.AddChild(box);
            var stripe = new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(1.15f, 0.15f, 0.28f) },
                Position = new Vector3(0f, 0.35f, 0f),
                MaterialOverride = MaterialFactory.Emissive(Palette.NeonPink, 2.5f)
            };
            packRoot.AddChild(stripe);
            VisualManager.EnforceCelShading(packRoot);
        }

        private async void OnOpen()
        {
            var result = DarknetGacha.OpenPack(GameManager.Instance.Save, GameManager.Instance.Economy, GameManager.Instance.Random);
            if (!result.Success)
            {
                SetStatus(result.Message, true);
                return;
            }

            Persist();
            Audio.AudioBus.Play("pack");
            var pull = result.Pull!;
            Color glow = Palette.ForRarity(pull.Rarity);
            await PlayOpenVfx(glow, pull.Rarity == Rarity.GenCorrupted);
            string color = UiKit.Hex(glow);
            string tag = pull.Rarity == Rarity.GenCorrupted ? "GEN-CORRUPTED" : pull.Rarity.DisplayName();
            resultLabel.Text =
                $"[color=#{color}][b]{pull.DisplayName}[/b][/color]  ·  {tag}\n" +
                $"{result.Message}\n" +
                $"Packs geöffnet: {GameManager.Instance.Save.PacksOpened}";
            SetStatus(result.Message);
        }

        private async System.Threading.Tasks.Task PlayOpenVfx(Color accent, bool corrupted)
        {
            BuildPackMesh();
            foreach (var child in packRoot.GetChildren())
            {
                if (child is MeshInstance3D { MaterialOverride: StandardMaterial3D mat } && mat.EmissionEnabled)
                    mat.Emission = accent;
            }
            var tween = CreateTween();
            tween.TweenProperty(packRoot, "rotation_degrees:y", corrupted ? 720f : 360f, corrupted ? 0.75 : 0.55)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenProperty(packRoot, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.35);
            tween.TweenProperty(packRoot, "scale", Vector3.One, 0.2);
            await ToSignal(tween, Tween.SignalName.Finished);
            packRoot.RotationDegrees = Vector3.Zero;
            if (corrupted)
                Visuals.CameraJuice.HitStop(this, 0.1, 0.3f);
        }
    }
}
