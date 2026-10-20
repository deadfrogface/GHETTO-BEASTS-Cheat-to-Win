using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>Helpers for assembling low-poly placeholder models from primitive meshes.</summary>
    public static class Shapes
    {
        public static MeshInstance3D Part(
            Node3D parent, Mesh mesh, Vector3 position, Material material,
            Vector3? rotationDegrees = null, Vector3? scale = null)
        {
            var instance = new MeshInstance3D
            {
                Mesh = mesh,
                MaterialOverride = material,
                Position = position,
                RotationDegrees = rotationDegrees ?? Vector3.Zero,
                Scale = scale ?? Vector3.One
            };
            parent.AddChild(instance);
            return instance;
        }

        public static MeshInstance3D Toon(
            Node3D parent, Mesh mesh, Vector3 position, Color color,
            Vector3? rotationDegrees = null, Vector3? scale = null, float outline = Core.VisualManager.DefaultOutlineWidth) =>
            Part(parent, mesh, position, MaterialFactory.Toon(color, outline), rotationDegrees, scale);

        public static SphereMesh Sphere(float radius) => new() { Radius = radius, Height = radius * 2f, RadialSegments = 24, Rings = 12 };

        public static CapsuleMesh Capsule(float radius, float height) => new() { Radius = radius, Height = height, RadialSegments = 24, Rings = 6 };

        public static CylinderMesh Cylinder(float top, float bottom, float height) =>
            new() { TopRadius = top, BottomRadius = bottom, Height = height, RadialSegments = 16 };

        public static BoxMesh Box(float x, float y, float z) => new() { Size = new Vector3(x, y, z) };

        public static CylinderMesh Cone(float radius, float height) => Cylinder(0f, radius, height);

        /// <summary>Simple humanoid facing +Z with feet at y = 0. Returns the head node.</summary>
        public static Node3D Humanoid(Node3D parent, Color outfit, Color skin, Color accent, bool hat = false)
        {
            Toon(parent, Cylinder(0.09f, 0.08f, 0.8f), new Vector3(-0.13f, 0.4f, 0f), outfit.Darkened(0.35f));
            Toon(parent, Cylinder(0.09f, 0.08f, 0.8f), new Vector3(0.13f, 0.4f, 0f), outfit.Darkened(0.35f));
            Toon(parent, Capsule(0.27f, 0.85f), new Vector3(0f, 1.15f, 0f), outfit);
            Toon(parent, Cylinder(0.07f, 0.06f, 0.7f), new Vector3(-0.36f, 1.15f, 0.05f), outfit, new Vector3(10f, 0f, 12f));
            Toon(parent, Cylinder(0.07f, 0.06f, 0.7f), new Vector3(0.36f, 1.15f, 0.05f), outfit, new Vector3(10f, 0f, -12f));

            var head = new Node3D { Position = new Vector3(0f, 1.78f, 0f) };
            parent.AddChild(head);
            Toon(head, Sphere(0.21f), Vector3.Zero, skin);
            Part(head, Sphere(0.035f), new Vector3(-0.08f, 0.03f, 0.18f), MaterialFactory.Emissive(accent, 2.2f));
            Part(head, Sphere(0.035f), new Vector3(0.08f, 0.03f, 0.18f), MaterialFactory.Emissive(accent, 2.2f));
            if (hat)
            {
                Toon(head, Cylinder(0.24f, 0.24f, 0.05f), new Vector3(0f, 0.15f, 0f), outfit.Darkened(0.5f));
                Toon(head, Cylinder(0.16f, 0.18f, 0.2f), new Vector3(0f, 0.27f, 0f), outfit.Darkened(0.5f));
            }
            return head;
        }
    }
}
