using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Top-down / isometric alley mover with optional virtual joystick.</summary>
    public partial class PlayerController : CharacterBody3D
    {
        [Export] public float MoveSpeed { get; set; } = 6.5f;

        private Node3D model = null!;
        private Vector2 joystick;
        private bool joystickActive;

        public override void _Ready()
        {
            var col = new CollisionShape3D { Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.4f }, Position = new Vector3(0f, 0.7f, 0f) };
            AddChild(col);
            model = new Node3D { Name = "Model" };
            AddChild(model);
            Visuals.Shapes.Humanoid(model, new Color(0.22f, 0.4f, 0.46f), Visuals.Palette.Skin, Visuals.Palette.NeonCyan);
            Core.VisualManager.EnforceCelShading(model);
        }

        public void SetJoystick(Vector2 value)
        {
            joystick = value.LengthSquared() > 1f ? value.Normalized() : value;
            joystickActive = joystick.LengthSquared() > 0.01f;
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 input = joystickActive ? joystick : Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            // WASD fallback
            if (!joystickActive)
            {
                if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) input.X -= 1;
                if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) input.X += 1;
                if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) input.Y -= 1;
                if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) input.Y += 1;
                if (input.LengthSquared() > 1f) input = input.Normalized();
            }

            Vector3 dir = new(input.X, 0f, input.Y);
            Velocity = dir * MoveSpeed;
            MoveAndSlide();

            // Clamp to alley bounds
            var p = GlobalPosition;
            p.X = Mathf.Clamp(p.X, -11f, 11f);
            p.Z = Mathf.Clamp(p.Z, -12f, 12f);
            p.Y = 0f;
            GlobalPosition = p;

            if (dir.LengthSquared() > 0.01f)
                model.LookAt(GlobalPosition + dir, Vector3.Up, true);
        }
    }
}
