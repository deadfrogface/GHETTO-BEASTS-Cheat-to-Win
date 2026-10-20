using Godot;

namespace GhettoBeasts.District
{
    /// <summary>
    /// Third-person alley mover: camera-relative WASD / stick, mesh faces travel dir.
    /// Pattern inspired by vaporvee gd-net-thirdpersoncontroller (MIT) — minimal C# port.
    /// </summary>
    public partial class PlayerController : CharacterBody3D
    {
        [Export] public float MoveSpeed { get; set; } = 6.8f;
        [Export] public float TurnSpeed { get; set; } = 12f;

        private Node3D model = null!;
        private Vector2 joystick;
        private bool joystickActive;
        private float cameraYaw;
        private bool inputLocked;
        private float minX = -11f;
        private float maxX = 11f;
        private float minZ = -12f;
        private float maxZ = 12f;

        public Node3D Model => model;
        public float CameraYaw => cameraYaw;

        public void SetPlayBounds(Rect2 bounds)
        {
            minX = bounds.Position.X;
            minZ = bounds.Position.Y;
            maxX = bounds.Position.X + bounds.Size.X;
            maxZ = bounds.Position.Y + bounds.Size.Y;
        }

        public override void _Ready()
        {
            CollisionLayer = 1;
            CollisionMask = 1;

            var col = new CollisionShape3D
            {
                Shape = new CapsuleShape3D { Radius = 0.35f, Height = 1.4f },
                Position = new Vector3(0f, 0.7f, 0f)
            };
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

        public void SetCameraYaw(float yawRadians) => cameraYaw = yawRadians;

        public void SetInputLocked(bool locked)
        {
            inputLocked = locked;
            if (locked) Velocity = Vector3.Zero;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (inputLocked)
            {
                Velocity = Vector3.Zero;
                MoveAndSlide();
                return;
            }

            Vector2 input = joystickActive ? joystick : ReadKeyboard();
            // Camera-relative XZ (vaporvee pattern)
            var basis = Basis.FromEuler(new Vector3(0f, cameraYaw, 0f));
            Vector3 dir = (basis * new Vector3(input.X, 0f, input.Y)).Normalized();
            if (input.LengthSquared() < 0.01f) dir = Vector3.Zero;

            Velocity = dir * MoveSpeed;
            MoveAndSlide();

            var p = GlobalPosition;
            p.X = Mathf.Clamp(p.X, minX, maxX);
            p.Z = Mathf.Clamp(p.Z, minZ, maxZ);
            p.Y = 0f;
            GlobalPosition = p;

            if (dir.LengthSquared() > 0.01f)
            {
                float targetYaw = Mathf.Atan2(dir.X, dir.Z);
                float current = model.Rotation.Y;
                float next = Mathf.LerpAngle(current, targetYaw, 1f - Mathf.Exp(-TurnSpeed * (float)delta));
                model.Rotation = new Vector3(0f, next, 0f);
            }
        }

        private static Vector2 ReadKeyboard()
        {
            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left)) input.X -= 1;
            if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right)) input.X += 1;
            if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up)) input.Y -= 1;
            if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down)) input.Y += 1;
            if (input.LengthSquared() > 1f) input = input.Normalized();
            return input;
        }
    }
}
