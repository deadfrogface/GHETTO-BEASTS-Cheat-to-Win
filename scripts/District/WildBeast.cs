using System.Linq;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Hostile overworld encounter — contact or attack range triggers seamless battle.</summary>
    public partial class WildBeast : CharacterBody3D
    {
        [Signal] public delegate void EncounterTriggeredEventHandler(string opponentId);

        [Export] public float AggroRadius { get; set; } = 1.55f;
        [Export] public float WanderSpeed { get; set; } = 1.4f;
        [Export] public string OpponentId { get; set; } = "kanalratte";

        private Node3D model = null!;
        private Node3D? player;
        private Label3D? bang;
        private Vector3 home;
        private Vector3 wanderTarget;
        private float retargetTimer;
        private bool fired;
        private bool isTrainer;

        public void Setup(string opponentId, Node3D playerNode, Vector3 spawn, bool trainer = false)
        {
            OpponentId = opponentId;
            player = playerNode;
            home = spawn;
            GlobalPosition = spawn;
            wanderTarget = spawn;
            isTrainer = trainer;
        }

        public override void _Ready()
        {
            var col = new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = 0.45f },
                Position = new Vector3(0f, 0.4f, 0f)
            };
            AddChild(col);
            CollisionLayer = 2;
            CollisionMask = 0;

            model = new Node3D { Name = "Model" };
            AddChild(model);
            BuildVisual();

            var area = new Area3D
            {
                Name = "Aggro",
                Monitoring = true,
                Monitorable = false,
                CollisionLayer = 0,
                CollisionMask = 1
            };
            area.AddChild(new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = AggroRadius },
                Position = new Vector3(0f, 0.4f, 0f)
            });
            AddChild(area);
            area.BodyEntered += OnBodyEntered;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (fired || player is null) return;

            retargetTimer -= (float)delta;
            if (retargetTimer <= 0f)
            {
                retargetTimer = (float)GD.RandRange(1.2, 2.8);
                wanderTarget = home + new Vector3(
                    (float)GD.RandRange(-2.2, 2.2),
                    0f,
                    (float)GD.RandRange(-2.2, 2.2));
            }

            Vector3 to = wanderTarget - GlobalPosition;
            to.Y = 0f;
            if (to.Length() > 0.2f)
            {
                Velocity = to.Normalized() * WanderSpeed;
                model.LookAt(GlobalPosition + to.Normalized(), Vector3.Up, true);
            }
            else
                Velocity = Vector3.Zero;

            MoveAndSlide();
            var p = GlobalPosition;
            p.Y = 0f;
            GlobalPosition = p;

            float dist = player.GlobalPosition.DistanceTo(GlobalPosition);
            if (bang is not null)
                bang.Visible = dist <= AggroRadius * 1.8f;
            if (dist <= AggroRadius)
                Trigger();
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController)
                Trigger();
        }

        private void Trigger()
        {
            if (fired) return;
            fired = true;
            EmitSignal(SignalName.EncounterTriggered, OpponentId);
        }

        private void BuildVisual()
        {
            if (!ContentCatalog.TryGetCard(OpponentId, out var def))
            {
                Shapes.Toon(model, Shapes.Sphere(0.4f), new Vector3(0f, 0.4f, 0f), Palette.Danger);
                VisualManager.EnforceCelShading(model);
                return;
            }

            var data = CardLibrary.Get(OpponentId);
            var beast = new Entities.Beast3D();
            model.AddChild(beast);
            beast.Build(def, data, Enumerable.Empty<MutationDefinition>(), Combat.Side.Enemy);
            if (beast.GetNodeOrNull<Node3D>("Model") is { } inner)
                inner.Scale = Vector3.One * Mathf.Max(0.1f, data.ModelScale) * 0.9f;

            bang = new Label3D
            {
                Text = isTrainer ? "!?" : "!",
                Position = new Vector3(0f, isTrainer ? 2.1f : 1.5f, 0f),
                FontSize = 48,
                Modulate = Palette.Danger,
                OutlineSize = 4,
                OutlineModulate = Palette.Outline,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                Visible = false
            };
            AddChild(bang);

            if (isTrainer)
            {
                var tag = new Label3D
                {
                    Text = "TRAINER",
                    Position = new Vector3(0f, 1.85f, 0f),
                    FontSize = 22,
                    Modulate = Palette.NeonYellow,
                    OutlineSize = 3,
                    OutlineModulate = Palette.Outline,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
                };
                AddChild(tag);
            }
        }
    }
}
