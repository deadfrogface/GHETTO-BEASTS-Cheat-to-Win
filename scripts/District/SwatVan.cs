using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Procedural SWAT van that rolls in when street heat hits 100%.</summary>
    public partial class SwatVan : Node3D
    {
        [Signal] public delegate void ArrivedEventHandler();

        private bool arrived;

        public override void _Ready()
        {
            // Body
            Shapes.Toon(this, Shapes.Box(2.4f, 1.1f, 4.2f), new Vector3(0f, 0.7f, 0f), new Color(0.12f, 0.14f, 0.22f), outline: 0.02f);
            Shapes.Toon(this, Shapes.Box(2.5f, 0.35f, 1.6f), new Vector3(0f, 1.35f, -0.4f), Palette.PoliceBlue.Darkened(0.35f), outline: 0.015f);
            // Light bar
            Shapes.Part(this, Shapes.Box(1.6f, 0.12f, 0.35f), new Vector3(0f, 1.55f, -0.2f),
                MaterialFactory.Emissive(Palette.Danger, 2.2f));
            Shapes.Part(this, Shapes.Box(0.5f, 0.12f, 0.35f), new Vector3(-0.7f, 1.55f, -0.2f),
                MaterialFactory.Emissive(Palette.PoliceBlue, 2.4f));
            // Wheels
            foreach (var wx in new[] { -0.95f, 0.95f })
            foreach (var wz in new[] { -1.3f, 1.3f })
                Shapes.Toon(this, Shapes.Cylinder(0.28f, 0.28f, 0.22f), new Vector3(wx, 0.28f, wz), Palette.Metal);

            var label = new Label3D
            {
                Text = "SWAT",
                Position = new Vector3(0f, 2.1f, 0f),
                FontSize = 48,
                Modulate = Palette.Danger,
                OutlineSize = 6,
                OutlineModulate = Palette.Outline,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };
            AddChild(label);
            VisualManager.EnforceCelShading(this);
        }

        public void RollIn(Vector3 from, Vector3 to, double duration = 1.4)
        {
            GlobalPosition = from;
            var tween = CreateTween();
            tween.TweenProperty(this, "global_position", to, duration)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenCallback(Callable.From(() =>
            {
                if (arrived) return;
                arrived = true;
                EmitSignal(SignalName.Arrived);
            }));
        }
    }
}
