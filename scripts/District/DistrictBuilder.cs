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
        public required List<Vector3> CopSpawns { get; init; }
    }

    /// <summary>One procedural alley block — asphalt, buildings, neon, hotspot pads.</summary>
    public static class DistrictBuilder
    {
        public static DistrictRefs Build(Node3D parent)
        {
            var root = new Node3D { Name = "District" };
            parent.AddChild(root);

            BuildEnvironment(root);
            BuildGround(root);
            BuildBuildings(root);
            BuildProps(root);

            var hotspots = new List<HotspotDef>
            {
                new() { Kind = HotspotKind.Arena, Label = "ARENA", Position = new Vector3(0f, 0.02f, -10f), Color = Palette.NeonPink, ScenePath = GameManager.CombatScene },
                new() { Kind = HotspotKind.Crispr, Label = "CRISPR", Position = new Vector3(-9f, 0.02f, -2f), Color = Palette.NeonPurple, ScenePath = GameManager.LabScene },
                new() { Kind = HotspotKind.Wash, Label = "WASH", Position = new Vector3(9f, 0.02f, -2f), Color = Palette.CleanCash, ScenePath = GameManager.WashScene },
                new() { Kind = HotspotKind.Dope, Label = "DOPE", Position = new Vector3(-9f, 0.02f, 6f), Color = Palette.NeonGreen, ScenePath = GameManager.DopeScene },
                new() { Kind = HotspotKind.Weap, Label = "WEAP", Position = new Vector3(9f, 0.02f, 6f), Color = Palette.Danger, ScenePath = GameManager.WeapScene },
                new() { Kind = HotspotKind.Darknet, Label = "DARKNET", Position = new Vector3(0f, 0.02f, 10f), Color = Palette.Diamond, ScenePath = GameManager.GachaScene },
                new() { Kind = HotspotKind.Hub, Label = "HUB", Position = new Vector3(0f, 0.02f, 0f), Color = Palette.NeonYellow, ScenePath = GameManager.HubScene }
            };

            foreach (var hs in hotspots)
                BuildHotspotPad(root, hs);

            var spawn = new Node3D { Name = "PlayerSpawn", Position = new Vector3(0f, 0f, 3f) };
            root.AddChild(spawn);

            var cops = new List<Vector3>
            {
                new(-4f, 0f, -6f),
                new(5f, 0f, 4f),
                new(-2f, 0f, 8f)
            };

            VisualManager.EnforceCelShading(root);
            return new DistrictRefs { Root = root, PlayerSpawn = spawn, Hotspots = hotspots, CopSpawns = cops };
        }

        private static void BuildEnvironment(Node3D root)
        {
            var environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = Palette.Background,
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.28f, 0.2f, 0.42f),
                AmbientLightEnergy = 0.5f,
                TonemapMode = Godot.Environment.ToneMapper.Filmic,
                GlowEnabled = true,
                GlowIntensity = 0.7f,
                FogEnabled = true,
                FogLightColor = new Color(0.1f, 0.04f, 0.18f),
                FogDensity = 0.02f
            };
            root.AddChild(new WorldEnvironment { Environment = environment });
            root.AddChild(new DirectionalLight3D
            {
                LightColor = new Color(0.75f, 0.7f, 1f),
                LightEnergy = 1.0f,
                RotationDegrees = new Vector3(-50f, 40f, 0f)
            });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonPink, LightEnergy = 2f, OmniRange = 12f, Position = new Vector3(-4f, 4f, 0f) });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonCyan, LightEnergy = 1.8f, OmniRange = 12f, Position = new Vector3(4f, 4f, 2f) });
        }

        private static void BuildGround(Node3D root)
        {
            Shapes.Toon(root, Shapes.Box(28f, 0.15f, 28f), new Vector3(0f, -0.08f, 0f), new Color(0.12f, 0.1f, 0.14f), outline: 0.01f);
            // alley stripes
            for (int i = -3; i <= 3; i++)
                Shapes.Toon(root, Shapes.Box(0.18f, 0.02f, 22f), new Vector3(i * 2.2f, 0.01f, 0f), new Color(0.35f, 0.32f, 0.2f), outline: 0f);
        }

        private static void BuildBuildings(Node3D root)
        {
            void Block(Vector3 pos, Vector3 size, Color color, Color neon)
            {
                Shapes.Toon(root, Shapes.Box(size.X, size.Y, size.Z), pos + new Vector3(0f, size.Y * 0.5f, 0f), color, outline: 0.025f);
                Shapes.Part(root, Shapes.Box(size.X * 0.9f, 0.12f, 0.08f), pos + new Vector3(0f, size.Y * 0.7f, size.Z * 0.5f + 0.05f),
                    MaterialFactory.Emissive(neon, 2.2f));
            }

            Block(new Vector3(-12f, 0f, -8f), new Vector3(5f, 6f, 4f), new Color(0.18f, 0.1f, 0.22f), Palette.NeonPurple);
            Block(new Vector3(12f, 0f, -8f), new Vector3(5f, 5f, 4f), new Color(0.15f, 0.12f, 0.2f), Palette.NeonCyan);
            Block(new Vector3(-12f, 0f, 8f), new Vector3(5f, 4.5f, 4f), new Color(0.2f, 0.08f, 0.12f), Palette.NeonPink);
            Block(new Vector3(12f, 0f, 8f), new Vector3(5f, 5.5f, 4f), new Color(0.1f, 0.14f, 0.18f), Palette.NeonGreen);
            Block(new Vector3(0f, 0f, -14f), new Vector3(8f, 7f, 3f), new Color(0.12f, 0.06f, 0.16f), Palette.NeonPink);
            Block(new Vector3(0f, 0f, 14f), new Vector3(6f, 4f, 3f), new Color(0.08f, 0.1f, 0.16f), Palette.Diamond);
        }

        private static void BuildProps(Node3D root)
        {
            for (int i = 0; i < 6; i++)
            {
                float x = -8f + i * 3.2f;
                Shapes.Toon(root, Shapes.Cylinder(0.12f, 0.12f, 3.2f), new Vector3(x, 1.6f, -4.5f), new Color(0.25f, 0.25f, 0.3f));
                Shapes.Part(root, Shapes.Sphere(0.25f), new Vector3(x, 3.3f, -4.5f), MaterialFactory.Emissive(i % 2 == 0 ? Palette.NeonPink : Palette.NeonCyan, 3f));
            }
            Shapes.Toon(root, Shapes.Box(1.2f, 1.0f, 0.8f), new Vector3(-3f, 0.5f, 2f), new Color(0.2f, 0.35f, 0.25f));
            Shapes.Toon(root, Shapes.Box(0.9f, 0.7f, 0.9f), new Vector3(4f, 0.35f, -1f), new Color(0.3f, 0.2f, 0.15f));
        }

        private static void BuildHotspotPad(Node3D root, HotspotDef hs)
        {
            var pad = new Node3D { Name = $"Hotspot_{hs.Kind}", Position = hs.Position };
            root.AddChild(pad);
            Shapes.Part(pad, new CylinderMesh { TopRadius = 1.3f, BottomRadius = 1.3f, Height = 0.04f, RadialSegments = 24 },
                Vector3.Zero, MaterialFactory.Emissive(hs.Color, 1.4f));
            var label = new Label3D
            {
                Text = hs.Label,
                Position = new Vector3(0f, 1.6f, 0f),
                FontSize = 48,
                Modulate = hs.Color,
                OutlineSize = 8,
                OutlineModulate = new Color(0.02f, 0f, 0.05f),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };
            pad.AddChild(label);
        }
    }
}
