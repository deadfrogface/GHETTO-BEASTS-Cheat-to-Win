using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>
    /// Companion AI with NavigationAgent3D when a nav map exists, else direct lag-follow.
    /// Idle bob / look-around when close to the player.
    /// </summary>
    public partial class BeastFollowAI : CharacterBody3D
    {
        [Export] public float FollowSpeed { get; set; } = 5.4f;
        [Export] public float MinDistance { get; set; } = 1.35f;
        [Export] public float MaxDistance { get; set; } = 8f;
        [Export] public float LagBehind { get; set; } = 1.85f;

        private Node3D? target;
        private Node3D model = null!;
        private NavigationAgent3D agent = null!;
        private string boundCardId = "";
        private float idleTimer;
        private float bobPhase;
        private bool useNav;

        public Vector3 WorldPosition => GlobalPosition;

        public void Bind(Node3D followTarget, string? cardId = null)
        {
            target = followTarget;
            string id = cardId ?? GameManager.Instance?.Save.ActiveFighterId ?? Roster.StarterCards[0];
            if (id == boundCardId && model.GetChildCount() > 0) return;
            boundCardId = id;
            RebuildVisual(id);
            if (target is not null)
                GlobalPosition = target.GlobalPosition + new Vector3(0.8f, 0f, 1.2f);
        }

        public override void _Ready()
        {
            var col = new CollisionShape3D
            {
                Shape = new CapsuleShape3D { Radius = 0.28f, Height = 0.9f },
                Position = new Vector3(0f, 0.45f, 0f),
                Disabled = true
            };
            AddChild(col);
            model = new Node3D { Name = "Model" };
            AddChild(model);
            agent = new NavigationAgent3D
            {
                PathDesiredDistance = 0.6f,
                TargetDesiredDistance = MinDistance,
                AvoidanceEnabled = false
            };
            AddChild(agent);
            CollisionLayer = 0;
            CollisionMask = 0;
            // Nav only if a map is baked; otherwise agent.GetNextPathPosition is unsafe.
            CallDeferred(nameof(DetectNav));
        }

        private void DetectNav()
        {
            useNav = NavigationServer3D.GetMaps().Count > 0;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (target is null || !GodotObject.IsInstanceValid(target)) return;

            string active = GameManager.Instance?.Save.ActiveFighterId ?? boundCardId;
            if (active != boundCardId)
                Bind(target, active);

            Vector3 behind = -target.GlobalTransform.Basis.Z;
            behind.Y = 0f;
            if (behind.LengthSquared() < 0.01f) behind = Vector3.Back;
            behind = behind.Normalized();

            Vector3 hang = target.GlobalPosition - behind * LagBehind + target.GlobalTransform.Basis.X * 0.55f;
            hang.Y = 0f;

            Vector3 to = hang - GlobalPosition;
            to.Y = 0f;
            float dist = to.Length();
            float dt = (float)delta;

            if (dist > MaxDistance)
            {
                GlobalPosition = hang - behind * 0.2f;
                Velocity = Vector3.Zero;
                idleTimer = 0f;
            }
            else if (dist > MinDistance)
            {
                idleTimer = 0f;
                Vector3 wish = to.Normalized() * FollowSpeed * Mathf.Clamp(dist / MinDistance, 1f, 2.4f);
                if (useNav)
                {
                    agent.TargetPosition = hang;
                    Vector3 next = agent.GetNextPathPosition();
                    Vector3 navDir = next - GlobalPosition;
                    navDir.Y = 0f;
                    if (navDir.LengthSquared() > 0.01f)
                        wish = navDir.Normalized() * wish.Length();
                }
                Velocity = wish;
                MoveAndSlide();
            }
            else
            {
                Velocity = Velocity.Lerp(Vector3.Zero, 1f - Mathf.Exp(-8f * dt));
                MoveAndSlide();
                Idle(dt);
            }

            var p = GlobalPosition;
            p.Y = 0f;
            GlobalPosition = p;

            Vector3 face = Velocity.LengthSquared() > 0.05f
                ? Velocity
                : (target.GlobalPosition - GlobalPosition);
            face.Y = 0f;
            if (face.LengthSquared() > 0.01f)
                model.LookAt(GlobalPosition + face.Normalized(), Vector3.Up, true);
        }

        private void Idle(float dt)
        {
            idleTimer += dt;
            bobPhase += dt * 3.2f;
            float bob = Mathf.Sin(bobPhase) * 0.04f;
            model.Position = new Vector3(0f, bob, 0f);
            if (idleTimer > 2.2f)
            {
                // Look around
                float yaw = Mathf.Sin(idleTimer * 1.4f) * 0.55f;
                model.Rotation = new Vector3(0f, yaw, 0f);
            }
        }

        private void RebuildVisual(string cardId)
        {
            foreach (var child in model.GetChildren())
                child.QueueFree();

            if (!ContentCatalog.TryGetCard(cardId, out var def))
            {
                Shapes.Toon(model, Shapes.Sphere(0.35f), new Vector3(0f, 0.35f, 0f), Palette.NeonCyan);
                VisualManager.EnforceCelShading(model);
                return;
            }

            var data = CardLibrary.Get(cardId);
            var mutDefs = Roster.ResolveMutations(GameManager.Instance!.Save, cardId);
            var beast = new Entities.Beast3D { Name = "CompanionBeast" };
            model.AddChild(beast);
            beast.Build(def, data, mutDefs, Combat.Side.Player);
            beast.Scale = Vector3.One * 0.85f;
            if (beast.GetNodeOrNull<Node3D>("Model") is { } inner)
                inner.Scale = Vector3.One * Mathf.Max(0.1f, data.ModelScale);
        }
    }
}
