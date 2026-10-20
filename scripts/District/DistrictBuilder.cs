using System.Collections.Generic;
using GhettoBeasts.Core;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.District
{
    public enum HotspotKind
    {
        Arena,
        Crispr,
        Wash,
        Dope,
        Weap,
        Darknet,
        Hub
    }

    public sealed class HotspotDef
    {
        public required HotspotKind Kind { get; init; }
        public required string Label { get; init; }
        public required Vector3 Position { get; init; }
        public required Color Color { get; init; }
        public required string ScenePath { get; init; }
    }

    public sealed class DistrictRefs
    {
        public required Node3D Root { get; init; }
        public required Node3D PlayerSpawn { get; init; }
        public required List<HotspotDef> Hotspots { get; init; }
        public required List<InteractableBuilding> Buildings { get; init; }
        public required List<Vector3> CopSpawns { get; init; }
        public required Node3D SwatSpawn { get; init; }
    }

    /// <summary>One procedural alley block — asphalt, physical buildings with Area3D doors, neon pads.</summary>
    public static class DistrictBuilder
    {
        public static DistrictRefs Build(Node3D parent)
        {
            var root = new Node3D { Name = "District" };
            parent.AddChild(root);

            BuildEnvironment(root);
            BuildGround(root);
            BuildPhysicalBlocks(root);

            var hotspots = new List<HotspotDef>
            {
                new() { Kind = HotspotKind.Arena, Label = "ARENA", Position = new Vector3(0f, 0.02f, -10f), Color = Palette.NeonPink, ScenePath = GameManager.CombatScene },
                new() { Kind = HotspotKind.Crispr, Label = "CRISPR", Position = new Vector3(-9f, 0.02f, -2f), Color = Palette.NeonPurple, ScenePath = GameManager.LabScene },
                new() { Kind = HotspotKind.Wash, Label = "WASH", Position = new Vector3(9f, 0.02f, -2f), Color = Palette.CleanCash, ScenePath = GameManager.WashScene },
                new() { Kind = HotspotKind.Dope, Label = "DOPE", Position = new Vector3(-9f, 0.02f, 6f), Color = Palette.NeonGreen, ScenePath = GameManager.DopeScene },
                new() { Kind = HotspotKind.Weap, Label = "WEAP", Position = new Vector3(9f, 0.02f, 6f), Color = Palette.Danger, ScenePath = GameManager.WeapScene },
                new() { Kind = HotspotKind.Darknet, Label = "DARKNET", Position = new Vector3(0f, 0.02f, 10f), Color = Palette.Diamond, ScenePath = GameManager.GachaScene }
                // Hub pad removed — meta UI lives on the Burner Phone (ESC), not a 2D hub button.
            };

            var buildings = new List<InteractableBuilding>();
            foreach (var hs in hotspots)
            {
                BuildHotspotPad(root, hs);
                buildings.Add(AttachDoor(root, hs));
            }

            BuildProps(root);

            var spawn = new Node3D { Name = "PlayerSpawn", Position = new Vector3(0f, 0f, 3f) };
            root.AddChild(spawn);

            var swatSpawn = new Node3D { Name = "SwatSpawn", Position = new Vector3(0f, 0f, -8f) };
            root.AddChild(swatSpawn);

            var cops = new List<Vector3>
            {
                new(-4f, 0f, -6f),
                new(5f, 0f, 4f),
                new(-2f, 0f, 8f)
            };

            VisualManager.EnforceCelShading(root);
            return new DistrictRefs
            {
                Root = root,
                PlayerSpawn = spawn,
                Hotspots = hotspots,
                Buildings = buildings,
                CopSpawns = cops,
                SwatSpawn = swatSpawn
            };
        }

        private static InteractableBuilding AttachDoor(Node3D root, HotspotDef hs)
        {
            var door = new InteractableBuilding
            {
                Name = $"Door_{hs.Kind}",
                Position = hs.Position + new Vector3(0f, 0.5f, 0f),
                BuildingLabel = hs.Label,
                ScenePath = hs.ScenePath,
                Kind = hs.Kind,
                Accent = hs.Color
            };
            door.AddChild(new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = 1.55f }
            });
            root.AddChild(door);
            return door;
        }

        private static void BuildEnvironment(Node3D root)
        {
            SceneEnvironmentFactory.Attach(root);
            root.AddChild(new DirectionalLight3D
            {
                LightColor = new Color(0.82f, 0.8f, 0.9f),
                LightEnergy = 0.95f,
                ShadowEnabled = true,
                RotationDegrees = new Vector3(-48f, 35f, 0f)
            });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonPink, LightEnergy = 1.0f, OmniRange = 12f, Position = new Vector3(-4f, 4f, 0f) });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonCyan, LightEnergy = 0.9f, OmniRange = 12f, Position = new Vector3(4f, 4f, 2f) });
        }

        private static void BuildGround(Node3D root)
        {
            Shapes.Toon(root, Shapes.Box(28f, 0.15f, 28f), new Vector3(0f, -0.08f, 0f), Palette.Ground, outline: 0.008f);
            for (int i = -3; i <= 3; i++)
                Shapes.Toon(root, Shapes.Box(0.18f, 0.02f, 22f), new Vector3(i * 2.2f, 0.01f, 0f), new Color(0.38f, 0.36f, 0.28f), outline: 0.006f);
        }

        private static void BuildPhysicalBlocks(Node3D root)
        {
            void Block(Vector3 pos, Vector3 size, Color color, Color neon)
            {
                Shapes.Toon(root, Shapes.Box(size.X, size.Y, size.Z), pos + new Vector3(0f, size.Y * 0.5f, 0f), color, outline: 0.016f);
                Shapes.Part(root, Shapes.Box(size.X * 0.9f, 0.12f, 0.08f), pos + new Vector3(0f, size.Y * 0.7f, size.Z * 0.5f + 0.05f),
                    MaterialFactory.Emissive(neon, 1.4f));
                // Static collision so player bumps into facades
                var body = new StaticBody3D { Position = pos + new Vector3(0f, size.Y * 0.5f, 0f) };
                body.AddChild(new CollisionShape3D
                {
                    Shape = new BoxShape3D { Size = size }
                });
                root.AddChild(body);
            }

            Block(new Vector3(-12f, 0f, -8f), new Vector3(5f, 6f, 4f), Palette.Building, Palette.NeonPurple);
            Block(new Vector3(12f, 0f, -8f), new Vector3(5f, 5f, 4f), Palette.BuildingAlt, Palette.NeonCyan);
            Block(new Vector3(-12f, 0f, 8f), new Vector3(5f, 4.5f, 4f), new Color(0.3f, 0.24f, 0.26f), Palette.NeonPink);
            Block(new Vector3(12f, 0f, 8f), new Vector3(5f, 5.5f, 4f), new Color(0.22f, 0.28f, 0.3f), Palette.NeonGreen);
            Block(new Vector3(0f, 0f, -14f), new Vector3(8f, 7f, 3f), new Color(0.26f, 0.24f, 0.3f), Palette.NeonPink);
            Block(new Vector3(0f, 0f, 14f), new Vector3(6f, 4f, 3f), new Color(0.22f, 0.26f, 0.32f), Palette.Diamond);
        }

        private static void BuildProps(Node3D root)
        {
            for (int i = 0; i < 6; i++)
            {
                float x = -8f + i * 3.2f;
                Shapes.Toon(root, Shapes.Cylinder(0.12f, 0.12f, 3.2f), new Vector3(x, 1.6f, -4.5f), Palette.Metal);
                Shapes.Part(root, Shapes.Sphere(0.25f), new Vector3(x, 3.3f, -4.5f),
                    MaterialFactory.Emissive(i % 2 == 0 ? Palette.NeonPink : Palette.NeonCyan, 1.8f));
            }
            Shapes.Toon(root, Shapes.Box(1.2f, 1.0f, 0.8f), new Vector3(-3f, 0.5f, 2f), new Color(0.28f, 0.36f, 0.3f));
            Shapes.Toon(root, Shapes.Box(0.9f, 0.7f, 0.9f), new Vector3(4f, 0.35f, -1f), new Color(0.36f, 0.3f, 0.26f));
        }

        private static void BuildHotspotPad(Node3D root, HotspotDef hs)
        {
            var pad = new Node3D { Name = $"Hotspot_{hs.Kind}", Position = hs.Position };
            root.AddChild(pad);
            Shapes.Part(pad, new CylinderMesh { TopRadius = 1.3f, BottomRadius = 1.3f, Height = 0.04f, RadialSegments = 24 },
                Vector3.Zero, MaterialFactory.Emissive(hs.Color, 0.9f));
            var label = new Label3D
            {
                Text = hs.Label,
                Position = new Vector3(0f, 1.6f, 0f),
                FontSize = 42,
                Modulate = hs.Color.Lightened(0.1f),
                OutlineSize = 6,
                OutlineModulate = Palette.Outline,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };
            pad.AddChild(label);
        }
    }
}
