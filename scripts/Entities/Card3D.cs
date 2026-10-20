using System.Collections.Generic;
using System.Threading.Tasks;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Entities
{
    /// <summary>Holographic summon card: appears in the trainer's hand, gets slammed onto the asphalt and dissolves.</summary>
    public partial class Card3D : Node3D
    {
        private static readonly Vector2 CardSize = new(0.72f, 1.02f);

        private readonly List<Label3D> labels = new();
        private ShaderMaterial material = null!;
        private Color frameColor;

        public void Setup(CardDefinition definition, CardData data)
        {
            Name = $"Card_{definition.Id}";
            frameColor = Palette.ForRarity(definition.Rarity);
            material = MaterialFactory.HoloCard(frameColor, data.PrimaryColor.Darkened(0.78f));

            AddChild(new MeshInstance3D { Mesh = new QuadMesh { Size = CardSize }, MaterialOverride = material });

            AddLabel(definition.Rarity.DisplayName().ToUpperInvariant(), new Vector3(0f, 0.44f, 0.004f), 26, frameColor);
            AddLabel(definition.Kind == CardKind.Beast ? "BESTIE" : "SÖLDNER", new Vector3(0f, 0.37f, 0.004f), 16, Palette.TextDim);

            if (data.Artwork is not null)
            {
                float targetWidth = CardSize.X * 0.74f;
                var sprite = new Sprite3D
                {
                    Texture = data.Artwork,
                    PixelSize = targetWidth / Mathf.Max(1, data.Artwork.GetWidth()),
                    Position = new Vector3(0f, 0.11f, 0.003f)
                };
                AddChild(sprite);
            }
            else
            {
                AddLabel(definition.Name.Substring(0, 1), new Vector3(0f, 0.11f, 0.004f), 150, frameColor);
            }

            var name = AddLabel(definition.Name, new Vector3(0f, -0.17f, 0.004f), 26, Colors.White);
            name.Width = 300f;
            name.AutowrapMode = TextServer.AutowrapMode.WordSmart;

            var stats = definition.BaseStats;
            AddLabel($"HP {stats.MaxHp}   ATK {stats.Attack}   DEF {stats.Defense}", new Vector3(0f, -0.33f, 0.004f), 18, Palette.NeonCyan);
            AddLabel(definition.AbilityName, new Vector3(0f, -0.42f, 0.004f), 16, Palette.NeonPink);
        }

        /// <summary>Plays hand-reveal, fly-in and slam. Returns at impact; the card dissolves afterwards on its own.</summary>
        public async Task SummonAsync(Vector3 handPosition, Vector3 fieldPosition, Camera3D camera, CameraRig? rig = null)
        {
            GlobalPosition = handPosition;
            LookAt(camera.GlobalPosition, Vector3.Up, true);
            Scale = Vector3.One * 0.05f;

            var reveal = CreateTween().SetParallel();
            reveal.TweenProperty(this, "scale", Vector3.One, 0.3).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            reveal.TweenProperty(material, "shader_parameter/glow", 1.6f, 0.4).From(5f);
            await ToSignal(reveal, Tween.SignalName.Finished);
            await Vfx.Delay(this, 0.3);

            Vector3 above = fieldPosition + Vector3.Up * 2.1f;
            var fly = CreateTween().SetParallel();
            fly.TweenProperty(this, "global_position", above, 0.32).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
            fly.TweenProperty(this, "rotation:y", Rotation.Y + Mathf.Tau, 0.32);
            await ToSignal(fly, Tween.SignalName.Finished);

            var slam = CreateTween().SetParallel();
            slam.TweenProperty(this, "global_position", fieldPosition + Vector3.Up * 0.02f, 0.13).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
            slam.TweenProperty(this, "rotation", new Vector3(-Mathf.Pi / 2f, Rotation.Y, 0f), 0.13);
            await ToSignal(slam, Tween.SignalName.Finished);

            var parent = GetParent();
            Vfx.Burst(parent, fieldPosition + Vector3.Up * 0.1f, frameColor, 90, 7f, 0.9f, 0.05f);
            Vfx.Burst(parent, fieldPosition + Vector3.Up * 0.1f, Colors.White, 40, 10f, 0.4f, 0.025f, -2f);
            Vfx.Smoke(parent, fieldPosition);
            Vfx.Flash(parent, fieldPosition + Vector3.Up * 0.8f, frameColor, 10f, 0.5f);
            rig?.Shake(0.22f, 0.35);
            rig?.Punch();

            var dissolve = CreateTween().SetParallel();
            dissolve.TweenProperty(material, "shader_parameter/dissolve", 1f, 0.55);
            dissolve.TweenProperty(this, "scale", Vector3.One * 1.25f, 0.55);
            foreach (var label in labels)
            {
                dissolve.TweenProperty(label, "modulate:a", 0f, 0.3);
                dissolve.TweenProperty(label, "outline_modulate:a", 0f, 0.3);
            }
            dissolve.Chain().TweenCallback(Callable.From(QueueFree));
        }

        private Label3D AddLabel(string text, Vector3 position, int fontSize, Color color)
        {
            var label = new Label3D
            {
                Text = text,
                FontSize = fontSize,
                PixelSize = 0.0022f,
                Position = position,
                Modulate = color,
                OutlineSize = 6,
                OutlineModulate = new Color(0.02f, 0f, 0.05f)
            };
            AddChild(label);
            labels.Add(label);
            return label;
        }
    }
}
