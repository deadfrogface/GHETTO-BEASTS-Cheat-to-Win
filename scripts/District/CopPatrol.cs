using System;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Patrol cop with a colored ground vision cone. Detection triggers combat.</summary>
    public partial class CopPatrol : Node3D
    {
        [Signal] public delegate void PlayerSpottedEventHandler();

        [Export] public float PatrolSpeed { get; set; } = 2.2f;
        [Export] public float VisionRange { get; set; } = 4.2f;
        [Export] public float VisionHalfAngleDeg { get; set; } = 38f;

        private Node3D body = null!;
        private MeshInstance3D cone = null!;
        private Vector3[] waypoints = Array.Empty<Vector3>();
        private int waypointIndex;
        private Node3D? player;
        private float alertPulse;

        public void Setup(Vector3[] path, Node3D playerNode)
        {
            waypoints = path;
            player = playerNode;
            GlobalPosition = path[0];
        }

        public override void _Ready()
        {
            body = new Node3D { Name = "Body" };
            AddChild(body);
            Shapes.Humanoid(body, new Color(0.1f, 0.15f, 0.35f), new Color(0.85f, 0.7f, 0.6f), Palette.PoliceBlue, hat: true);
            Core.VisualManager.EnforceCelShading(body);

            cone = new MeshInstance3D
            {
                Mesh = new CylinderMesh { TopRadius = 0.05f, BottomRadius = VisionRange * 0.85f, Height = 0.03f, RadialSegments = 24 },
                Position = new Vector3(0f, 0.03f, VisionRange * 0.4f),
                RotationDegrees = new Vector3(0f, 0f, 0f),
                MaterialOverride = MaterialFactory.Emissive(new Color(0.2f, 0.45f, 1f, 0.55f), 1.2f)
            };
            // Flatten as ground indicator in front of cop
            cone.Scale = new Vector3(1f, 1f, 1.15f);
            AddChild(cone);
        }

        public override void _Process(double delta)
        {
            if (waypoints.Length == 0) return;

            Vector3 target = waypoints[waypointIndex];
            Vector3 to = target - GlobalPosition;
            to.Y = 0f;
            if (to.Length() < 0.35f)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            else
            {
                GlobalPosition += to.Normalized() * PatrolSpeed * (float)delta;
                LookAt(GlobalPosition + to.Normalized(), Vector3.Up, true);
            }

            alertPulse = Mathf.Max(0f, alertPulse - (float)delta);
            bool spotted = IsPlayerInVision();
            Color coneColor = spotted ? new Color(1f, 0.2f, 0.25f, 0.7f)
                : alertPulse > 0f ? new Color(1f, 0.7f, 0.15f, 0.6f)
                : new Color(0.2f, 0.45f, 1f, 0.45f);
            cone.MaterialOverride = MaterialFactory.Emissive(coneColor, spotted ? 2.5f : 1.2f);

            if (spotted)
                EmitSignal(SignalName.PlayerSpotted);
        }

        private bool IsPlayerInVision()
        {
            if (player is null) return false;
            Vector3 toPlayer = player.GlobalPosition - GlobalPosition;
            toPlayer.Y = 0f;
            float dist = toPlayer.Length();
            if (dist > VisionRange || dist < 0.05f) return false;
            Vector3 forward = -GlobalTransform.Basis.Z;
            forward.Y = 0f;
            forward = forward.Normalized();
            float angle = Mathf.RadToDeg(Mathf.Acos(Mathf.Clamp(forward.Dot(toPlayer.Normalized()), -1f, 1f)));
            if (angle <= VisionHalfAngleDeg)
            {
                alertPulse = 0.4f;
                return true;
            }
            return false;
        }
    }
}
