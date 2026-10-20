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
        public required string UnlockId { get; init; }
        public bool StartsLocked { get; init; }
    }

    public sealed class DistrictRefs
    {
        public required Node3D Root { get; init; }
        public required Node3D PlayerSpawn { get; init; }
        public required List<HotspotDef> Hotspots { get; init; }
        public required List<InteractableBuilding> Buildings { get; init; }
        public required List<Vector3> CopSpawns { get; init; }
        public required List<Vector3> WildSpots { get; init; }
        public required List<Vector3> TrainerSpots { get; init; }
        public required Node3D SwatSpawn { get; init; }
        public required Rect2 PlayBounds { get; init; }
        public bool BackAlleyOpen { get; init; }
        public bool DockBlockOpen { get; init; }
    }

    /// <summary>Multi-block procedural district — main alley + optional back alley / dock block.</summary>
    public static class DistrictBuilder
    {
        public static DistrictRefs Build(Node3D parent, SaveData? save = null)
        {
            save ??= GameManager.Instance?.Save ?? SaveData.CreateDefault();
            StreetProgression.EnsureDefaults(save);
            bool backOpen = StreetProgression.IsUnlocked(save, StreetProgression.BackAlley);
            bool dockOpen = StreetProgression.IsUnlocked(save, StreetProgression.DockBlock);

            var root = new Node3D { Name = "District" };
            parent.AddChild(root);

            BuildEnvironment(root);
            BuildGround(root, backOpen, dockOpen);
            BuildPhysicalBlocks(root, backOpen, dockOpen);

            var hotspots = new List<HotspotDef>
            {
                new() { Kind = HotspotKind.Arena, Label = "ARENA", Position = new Vector3(0f, 0.02f, -10f), Color = Palette.NeonPink, ScenePath = GameManager.CombatScene, UnlockId = StreetProgression.Arena },
                new() { Kind = HotspotKind.Crispr, Label = "CRISPR", Position = new Vector3(-9f, 0.02f, -2f), Color = Palette.NeonPurple, ScenePath = GameManager.LabScene, UnlockId = StreetProgression.Crispr },
                new() { Kind = HotspotKind.Wash, Label = "WASH", Position = new Vector3(9f, 0.02f, -2f), Color = Palette.CleanCash, ScenePath = GameManager.WashScene, UnlockId = StreetProgression.Wash },
                new() { Kind = HotspotKind.Dope, Label = "DOPE", Position = new Vector3(-9f, 0.02f, 6f), Color = Palette.NeonGreen, ScenePath = GameManager.DopeScene, UnlockId = StreetProgression.Dope, StartsLocked = true },
                new() { Kind = HotspotKind.Weap, Label = "WEAP", Position = new Vector3(9f, 0.02f, 6f), Color = Palette.Danger, ScenePath = GameManager.WeapScene, UnlockId = StreetProgression.Weap, StartsLocked = true },
                new() { Kind = HotspotKind.Darknet, Label = "DARKNET", Position = new Vector3(0f, 0.02f, 10f), Color = Palette.Diamond, ScenePath = GameManager.GachaScene, UnlockId = StreetProgression.Darknet }
            };

            if (backOpen)
            {
                hotspots.Add(new HotspotDef
                {
                    Kind = HotspotKind.Arena,
                    Label = "PIT B",
                    Position = new Vector3(-22f, 0.02f, 0f),
                    Color = Palette.NeonYellow,
                    ScenePath = GameManager.CombatScene,
                    UnlockId = StreetProgression.BackAlley
                });
            }

            if (dockOpen)
            {
                hotspots.Add(new HotspotDef
                {
                    Kind = HotspotKind.Darknet,
                    Label = "DOCK NET",
                    Position = new Vector3(22f, 0.02f, 2f),
                    Color = Palette.Diamond.Lightened(0.2f),
                    ScenePath = GameManager.GachaScene,
                    UnlockId = StreetProgression.DockBlock
                });
            }

            var buildings = new List<InteractableBuilding>();
            foreach (var hs in hotspots)
            {
                bool unlocked = StreetProgression.IsUnlocked(save, hs.UnlockId);
                BuildHotspotPad(root, hs, unlocked);
                buildings.Add(AttachDoor(root, hs, unlocked));
            }

            BuildProps(root, backOpen, dockOpen);
            TryDropImportedProps(root);

            var spawn = new Node3D { Name = "PlayerSpawn", Position = new Vector3(0f, 0f, 3f) };
            root.AddChild(spawn);

            var swatSpawn = new Node3D { Name = "SwatSpawn", Position = new Vector3(0f, 0f, -8f) };
            root.AddChild(swatSpawn);

            var cops = new List<Vector3>
            {
                new(-4f, 0f, -6f),
                new(5f, 0f, 4f),
                new(-2f, 0f, 8f),
                new(8f, 0f, -9f)
            };
            if (backOpen) cops.Add(new Vector3(-20f, 0f, -3f));
            if (dockOpen) cops.Add(new Vector3(20f, 0f, 6f));

            var wild = new List<Vector3>
            {
                new(-6f, 0f, -4f),
                new(6.5f, 0f, 1f),
                new(-3f, 0f, 7f),
                new(3.5f, 0f, -7f),
                new(-1f, 0f, 0f),
                new(7f, 0f, 8f),
                new(-8f, 0f, -9f)
            };
            if (backOpen)
            {
                wild.Add(new Vector3(-18f, 0f, 2f));
                wild.Add(new Vector3(-24f, 0f, -4f));
                wild.Add(new Vector3(-21f, 0f, 5f));
            }
            if (dockOpen)
            {
                wild.Add(new Vector3(18f, 0f, -2f));
                wild.Add(new Vector3(24f, 0f, 5f));
            }

            var trainers = new List<Vector3>
            {
                new(7f, 0f, -5f),
                new(-7.5f, 0f, 2.5f),
                new(2f, 0f, 9f)
            };
            if (backOpen) trainers.Add(new Vector3(-23f, 0f, 1f));
            if (dockOpen)
            {
                trainers.Add(new Vector3(21f, 0f, -3f));
                trainers.Add(new Vector3(19f, 0f, 7f));
            }

            float minX = backOpen ? -28f : -12f;
            float maxX = dockOpen ? 28f : 12f;
            float minZ = -14f;
            float maxZ = 14f;

            VisualManager.EnforceCelShading(root);
            return new DistrictRefs
            {
                Root = root,
                PlayerSpawn = spawn,
                Hotspots = hotspots,
                Buildings = buildings,
                CopSpawns = cops,
                WildSpots = wild,
                TrainerSpots = trainers,
                SwatSpawn = swatSpawn,
                PlayBounds = new Rect2(minX, minZ, maxX - minX, maxZ - minZ),
                BackAlleyOpen = backOpen,
                DockBlockOpen = dockOpen
            };
        }

        private static InteractableBuilding AttachDoor(Node3D root, HotspotDef hs, bool unlocked)
        {
            var door = new InteractableBuilding
            {
                Name = $"Door_{hs.Kind}_{hs.UnlockId}",
                Position = hs.Position + new Vector3(0f, 0.5f, 0f),
                BuildingLabel = unlocked ? hs.Label : $"{hs.Label} (LOCKED)",
                ScenePath = hs.ScenePath,
                Kind = hs.Kind,
                Accent = unlocked ? hs.Color : Palette.TextDim,
                UnlockId = hs.UnlockId,
                IsUnlocked = unlocked
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
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonPink, LightEnergy = 1.0f, OmniRange = 14f, Position = new Vector3(-4f, 4f, 0f) });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonCyan, LightEnergy = 0.9f, OmniRange = 14f, Position = new Vector3(4f, 4f, 2f) });
            root.AddChild(new OmniLight3D { LightColor = Palette.NeonYellow, LightEnergy = 0.7f, OmniRange = 12f, Position = new Vector3(-20f, 3.5f, 0f) });
            root.AddChild(new OmniLight3D { LightColor = Palette.Diamond, LightEnergy = 0.7f, OmniRange = 12f, Position = new Vector3(20f, 3.5f, 2f) });
        }

        private static void BuildGround(Node3D root, bool backOpen, bool dockOpen)
        {
            float width = (backOpen ? 30f : 14f) + (dockOpen ? 16f : 0f);
            float centerX = (backOpen && dockOpen) ? 0f : backOpen ? -8f : dockOpen ? 8f : 0f;
            Shapes.Toon(root, Shapes.Box(Mathf.Max(28f, width + 8f), 0.15f, 32f), new Vector3(centerX, -0.08f, 0f), Palette.Ground, outline: 0.008f);

            // Main strip
            for (int i = -4; i <= 4; i++)
                Shapes.Toon(root, Shapes.Box(0.18f, 0.02f, 24f), new Vector3(i * 2.2f, 0.01f, 0f), new Color(0.38f, 0.36f, 0.28f), outline: 0.006f);

            // Cross alleys
            for (int z = -2; z <= 2; z++)
                Shapes.Toon(root, Shapes.Box(22f, 0.015f, 0.14f), new Vector3(0f, 0.015f, z * 4.5f), new Color(0.32f, 0.3f, 0.24f), outline: 0.005f);

            if (backOpen)
            {
                for (int i = 0; i < 5; i++)
                    Shapes.Toon(root, Shapes.Box(0.16f, 0.02f, 14f), new Vector3(-18f - i * 1.8f, 0.01f, 0f), new Color(0.3f, 0.28f, 0.22f), outline: 0.005f);
                var sign = new Label3D
                {
                    Text = "BACK ALLEY",
                    Position = new Vector3(-22f, 2.4f, -6f),
                    FontSize = 48,
                    Modulate = Palette.NeonYellow,
                    OutlineSize = 6,
                    OutlineModulate = Palette.Outline,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
                };
                root.AddChild(sign);
            }

            if (dockOpen)
            {
                for (int i = 0; i < 5; i++)
                    Shapes.Toon(root, Shapes.Box(0.16f, 0.02f, 14f), new Vector3(18f + i * 1.8f, 0.01f, 0f), new Color(0.26f, 0.3f, 0.34f), outline: 0.005f);
                var sign = new Label3D
                {
                    Text = "DOCK BLOCK",
                    Position = new Vector3(22f, 2.4f, -6f),
                    FontSize = 48,
                    Modulate = Palette.Diamond,
                    OutlineSize = 6,
                    OutlineModulate = Palette.Outline,
                    Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
                };
                root.AddChild(sign);
            }
        }

        private static void BuildPhysicalBlocks(Node3D root, bool backOpen, bool dockOpen)
        {
            void Block(Vector3 pos, Vector3 size, Color color, Color neon)
            {
                Shapes.Toon(root, Shapes.Box(size.X, size.Y, size.Z), pos + new Vector3(0f, size.Y * 0.5f, 0f), color, outline: 0.016f);
                Shapes.Part(root, Shapes.Box(size.X * 0.9f, 0.12f, 0.08f), pos + new Vector3(0f, size.Y * 0.7f, size.Z * 0.5f + 0.05f),
                    MaterialFactory.Emissive(neon, 1.4f));
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

            // Mid-block alleys
            Block(new Vector3(-6f, 0f, -12f), new Vector3(3f, 3.5f, 2.5f), new Color(0.28f, 0.22f, 0.24f), Palette.NeonPink);
            Block(new Vector3(6f, 0f, 12f), new Vector3(3f, 4f, 2.5f), new Color(0.2f, 0.24f, 0.28f), Palette.NeonCyan);

            if (backOpen)
            {
                Block(new Vector3(-26f, 0f, -6f), new Vector3(4f, 5f, 3.5f), new Color(0.24f, 0.2f, 0.18f), Palette.NeonYellow);
                Block(new Vector3(-26f, 0f, 6f), new Vector3(4f, 4f, 3.5f), new Color(0.2f, 0.18f, 0.16f), Palette.Danger);
                Block(new Vector3(-18f, 0f, -10f), new Vector3(3.5f, 3.2f, 2.5f), new Color(0.26f, 0.22f, 0.2f), Palette.NeonPink);
            }

            if (dockOpen)
            {
                Block(new Vector3(26f, 0f, -5f), new Vector3(4.5f, 5.5f, 3.5f), new Color(0.18f, 0.22f, 0.28f), Palette.Diamond);
                Block(new Vector3(26f, 0f, 7f), new Vector3(4f, 4.2f, 3.2f), new Color(0.16f, 0.2f, 0.24f), Palette.NeonCyan);
                Block(new Vector3(18f, 0f, 11f), new Vector3(3.5f, 3.5f, 2.8f), new Color(0.2f, 0.24f, 0.3f), Palette.NeonGreen);
            }
        }

        private static void BuildProps(Node3D root, bool backOpen, bool dockOpen)
        {
            for (int i = 0; i < 8; i++)
            {
                float x = -10f + i * 2.8f;
                Shapes.Toon(root, Shapes.Cylinder(0.12f, 0.12f, 3.2f), new Vector3(x, 1.6f, -4.5f), Palette.Metal);
                Shapes.Part(root, Shapes.Sphere(0.25f), new Vector3(x, 3.3f, -4.5f),
                    MaterialFactory.Emissive(i % 2 == 0 ? Palette.NeonPink : Palette.NeonCyan, 1.8f));
            }

            // Dumpsters / crates along both sidewalks
            Vector3[] dumpsters =
            {
                new(-3f, 0.5f, 2f), new(4f, 0.35f, -1f), new(-5f, 0.4f, -8f),
                new(5.5f, 0.45f, 5f), new(-2f, 0.4f, 11f), new(1.5f, 0.5f, -11f)
            };
            foreach (var p in dumpsters)
                Shapes.Toon(root, Shapes.Box(1.2f, 1.0f, 0.8f), p, new Color(0.28f, 0.36f, 0.3f));

            for (int i = 0; i < 4; i++)
                Shapes.Toon(root, Shapes.Box(0.7f, 0.7f, 0.7f), new Vector3(-8f + i * 1.1f, 0.35f, 3.5f), new Color(0.4f, 0.32f, 0.22f));

            if (backOpen)
            {
                for (int i = 0; i < 4; i++)
                {
                    Shapes.Toon(root, Shapes.Cylinder(0.1f, 0.1f, 2.8f), new Vector3(-20f + i * 2f, 1.4f, -2f), Palette.Metal);
                    Shapes.Part(root, Shapes.Sphere(0.2f), new Vector3(-20f + i * 2f, 2.9f, -2f),
                        MaterialFactory.Emissive(Palette.NeonYellow, 1.6f));
                }
                Shapes.Toon(root, Shapes.Box(1.4f, 1.1f, 0.9f), new Vector3(-19f, 0.55f, 3f), new Color(0.32f, 0.28f, 0.2f));
            }

            if (dockOpen)
            {
                for (int i = 0; i < 3; i++)
                    Shapes.Toon(root, Shapes.Cylinder(0.55f, 0.55f, 1.2f), new Vector3(17f + i * 2.2f, 0.6f, 0f), new Color(0.35f, 0.28f, 0.2f));
                Shapes.Toon(root, Shapes.Box(2.2f, 0.8f, 1.4f), new Vector3(23f, 0.4f, 4f), new Color(0.3f, 0.34f, 0.38f));
            }
        }

        private static void TryDropImportedProps(Node3D root)
        {
            // Prefer AssetPipeline GLBs when present (Khronos samples / future Kenney drops).
            string[] propIds = { "box_sample", "lantern_sample", "demo_box", "duck_sample" };
            Vector3[] spots =
            {
                new(2.5f, 0f, 2.2f),
                new(-4.5f, 0f, -2.5f),
                new(0.5f, 0f, -5.5f)
            };
            var pipe = AssetPipeline.Instance;
            if (pipe is null) return;
            for (int i = 0; i < spots.Length; i++)
            {
                string id = propIds[i % propIds.Length];
                var node = pipe.InstantiateModel(id, Palette.RimAccent, playIdle: false);
                if (node is null) continue;
                node.Name = $"Prop_{id}_{i}";
                root.AddChild(node);
                node.GlobalPosition = spots[i];
                node.Scale = Vector3.One * (id.Contains("lantern") ? 0.08f : 0.45f);
                VisualManager.EnforceCelShading(node);
            }
        }

        private static void BuildHotspotPad(Node3D root, HotspotDef hs, bool unlocked)
        {
            var pad = new Node3D { Name = $"Hotspot_{hs.Kind}_{hs.UnlockId}", Position = hs.Position };
            root.AddChild(pad);
            Color c = unlocked ? hs.Color : Palette.TextDim;
            Shapes.Part(pad, new CylinderMesh { TopRadius = 1.3f, BottomRadius = 1.3f, Height = 0.04f, RadialSegments = 24 },
                Vector3.Zero, MaterialFactory.Emissive(c, unlocked ? 0.9f : 0.25f));
            var label = new Label3D
            {
                Text = unlocked ? hs.Label : $"{hs.Label}\nLOCKED",
                Position = new Vector3(0f, 1.6f, 0f),
                FontSize = unlocked ? 42 : 32,
                Modulate = c.Lightened(0.1f),
                OutlineSize = 6,
                OutlineModulate = Palette.Outline,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
            };
            pad.AddChild(label);
        }
    }
}
