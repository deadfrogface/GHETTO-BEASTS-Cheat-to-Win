using Godot;

namespace GhettoBeasts.District
{
    /// <summary>Area3D door zone — walk in and press E (or tap prompt) to enter a building scene.</summary>
    public partial class InteractableBuilding : Area3D
    {
        [Export] public string BuildingLabel { get; set; } = "Gebäude";
        [Export] public string ScenePath { get; set; } = "";
        [Export] public HotspotKind Kind { get; set; } = HotspotKind.Arena;
        [Export] public Color Accent { get; set; } = Colors.White;

        public bool PlayerInside { get; private set; }

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            Monitoring = true;
            Monitorable = false;
            CollisionLayer = 0;
            CollisionMask = 1; // player layer
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is PlayerController)
                PlayerInside = true;
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is PlayerController)
                PlayerInside = false;
        }
    }
}
