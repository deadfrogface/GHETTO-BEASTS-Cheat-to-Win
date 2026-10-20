using System.Collections.Generic;
using Godot;

namespace GhettoBeasts.Visuals
{
    public sealed class ArenaRefs
    {
        public required Node3D PlayerTrainer { get; init; }
        public required Node3D EnemyTrainer { get; init; }
        public required Node3D Referee { get; init; }
        public required OmniLight3D[] RaidLights { get; init; }
        public Vector3 PlayerSpot { get; init; }
        public Vector3 EnemySpot { get; init; }

        private readonly Dictionary<Node3D, Transform3D> initialTransforms = new();

        public void CaptureInitialTransforms()
        {
            foreach (var node in new[] { PlayerTrainer, EnemyTrainer, Referee })
                initialTransforms[node] = node.Transform;
        }

        public void ResetActors()
        {
            foreach (var (node, transform) in initialTransforms)
                node.Transform = transform;
            foreach (var light in RaidLights) light.LightEnergy = 0f;
        }
    }

    /// <summary>Builds the underground backyard arena procedurally: asphalt, neon ring, buildings, lamps, crowd.</summary>
    public static class ArenaBuilder
    {
        private static readonly Vector3 ArenaCenter = Vector3.Zero;

        public static ArenaRefs Build(Node3D root)
        {
            var arena = new Node3D { Name = "Arena" };
            root.AddChild(arena);

            BuildEnvironment(arena);
            BuildGround(arena);
            BuildBuildings(arena);
            BuildProps(arena);
            BuildCrowd(arena);

            var playerTrainer = Trainer(arena, new Vector3(-4.3f, 0f, 3.1f), new Color(0.1f, 0.55f, 0.7f), Palette.NeonCyan, "DU");
            var enemyTrainer = Trainer(arena, new Vector3(4.3f, 0f, -3.1f), new Color(0.6f, 0.08f, 0.15f), Palette.Danger, "GEGNER");
            var referee = Trainer(arena, new Vector3(0.3f, 0f, -4.6f), new Color(0.85f, 0.85f, 0.85f), Palette.NeonYellow, "SCHIRI");

            var raidRed = new OmniLight3D { LightColor = Palette.Danger, LightEnergy = 0f, OmniRange = 16f, Position = new Vector3(-5f, 4f, 3f) };
            var raidBlue = new OmniLight3D { LightColor = Palette.PoliceBlue, LightEnergy = 0f, OmniRange = 16f, Position = new Vector3(5f, 4f, 3f) };
            arena.AddChild(raidRed);
            arena.AddChild(raidBlue);

            var refs = new ArenaRefs
            {
                PlayerTrainer = playerTrainer,
                EnemyTrainer = enemyTrainer,
                Referee = referee,
                RaidLights = new[] { raidRed, raidBlue },
                PlayerSpot = new Vector3(-2.1f, 0f, 1.2f),
                EnemySpot = new Vector3(2.1f, 0f, -1.2f)
            };
            refs.CaptureInitialTransforms();
            return refs;
        }

        private static void BuildEnvironment(Node3D arena)
        {
            var environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = Palette.Background,
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.32f, 0.22f, 0.5f),
                AmbientLightEnergy = 0.55f,
                TonemapMode = Godot.Environment.ToneMapper.Filmic,
                GlowEnabled = true,
                GlowIntensity = 0.9f,
                GlowBloom = 0.08f,
                GlowHdrThreshold = 0.85f,
                GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Additive,
                FogEnabled = true,
                FogLightColor = new Color(0.12f, 0.05f, 0.22f),
                FogDensity = 0.025f
            };
            arena.AddChild(new WorldEnvironment { Environment = environment });

            var moon = new DirectionalLight3D
            {
                LightColor = new Color(0.8f, 0.72f, 1f),
                LightEnergy = 1.15f,
                ShadowEnabled = true,
                RotationDegrees = new Vector3(-55f, -35f, 0f)
            };
            arena.AddChild(moon);

            arena.AddChild(new OmniLight3D { LightColor = Palette.NeonPink, LightEnergy = 2.2f, OmniRange = 9f, Position = new Vector3(-3f, 3f, -2f) });
            arena.AddChild(new OmniLight3D { LightColor = Palette.NeonCyan, LightEnergy = 2.0f, OmniRange = 9f, Position = new Vector3(3.5f, 3f, 2f) });
        }

        private static void BuildGround(Node3D arena)
        {
            Shapes.Toon(arena, new PlaneMesh { Size = new Vector2(48f, 48f) }, Vector3.Zero, new Color(0.11f, 0.1f, 0.14f), outline: 0f);

            var ring = MaterialFactory.Emissive(Palette.NeonPink, 3.5f);
            Shapes.Part(arena, new TorusMesh { InnerRadius = 3.95f, OuterRadius = 4.08f, Rings = 64, RingSegments = 6 }, new Vector3(0f, 0.01f, 0f), ring);
            Shapes.Part(arena, new TorusMesh { InnerRadius = 0.85f, OuterRadius = 0.92f, Rings = 32, RingSegments = 6 }, new Vector3(0f, 0.01f, 0f),
                MaterialFactory.Emissive(Palette.NeonCyan, 2.5f));
            Shapes.Part(arena, Shapes.Box(0.08f, 0.01f, 7.9f), new Vector3(0f, 0.012f, 0f), MaterialFactory.Emissive(Palette.NeonPink, 1.6f), new Vector3(0f, -30f, 0f));

            var puddle = new StandardMaterial3D { AlbedoColor = new Color(0.05f, 0.05f, 0.12f), Metallic = 0.9f, Roughness = 0.05f };
            Shapes.Part(arena, Shapes.Cylinder(1.2f, 1.2f, 0.01f), new Vector3(-5.5f, 0.005f, -1.5f), puddle, scale: new Vector3(1.4f, 1f, 0.8f));
            Shapes.Part(arena, Shapes.Cylinder(0.9f, 0.9f, 0.01f), new Vector3(5.8f, 0.005f, 2.6f), puddle, scale: new Vector3(1f, 1f, 1.5f));
        }

        private static void BuildBuildings(Node3D arena)
        {
            var rng = new RandomNumberGenerator { Seed = 1337 };
            Color[] windowColors = { Palette.NeonPink, Palette.NeonCyan, Palette.NeonYellow, new Color(1f, 0.55f, 0.25f) };

            void Building(Vector3 center, Vector3 size, Color color, bool frontFacesPositiveZ = true, bool sideways = false)
            {
                Shapes.Toon(arena, Shapes.Box(size.X, size.Y, size.Z), center + new Vector3(0f, size.Y / 2f, 0f), color, outline: 0.04f);
                float width = sideways ? size.Z : size.X;
                for (float y = 1.2f; y < size.Y - 0.6f; y += 1.25f)
                {
                    for (float u = -width / 2f + 0.7f; u < width / 2f - 0.4f; u += 1.1f)
                    {
                        if (rng.Randf() > 0.38f) continue;
                        var window = MaterialFactory.Emissive(windowColors[rng.RandiRange(0, windowColors.Length - 1)], rng.RandfRange(0.8f, 2.6f));
                        Vector3 position;
                        Vector3 rotation;
                        if (sideways)
                        {
                            float faceX = center.X + (frontFacesPositiveZ ? size.X / 2f + 0.01f : -size.X / 2f - 0.01f);
                            position = new Vector3(faceX, y, center.Z + u);
                            rotation = new Vector3(0f, frontFacesPositiveZ ? 90f : -90f, 0f);
                        }
                        else
                        {
                            position = new Vector3(center.X + u, y, center.Z + size.Z / 2f + 0.01f);
                            rotation = Vector3.Zero;
                        }
                        Shapes.Part(arena, new QuadMesh { Size = new Vector2(0.55f, 0.7f) }, position, window, rotation);
                    }
                }
            }

            Building(new Vector3(-11f, 0f, -11f), new Vector3(7f, 9f, 3f), new Color(0.16f, 0.11f, 0.22f));
            Building(new Vector3(-3.5f, 0f, -11.5f), new Vector3(7.5f, 6.5f, 3f), new Color(0.2f, 0.12f, 0.2f));
            Building(new Vector3(4.5f, 0f, -11f), new Vector3(7f, 11f, 3f), new Color(0.13f, 0.12f, 0.24f));
            Building(new Vector3(12f, 0f, -10.5f), new Vector3(6f, 8f, 3f), new Color(0.18f, 0.1f, 0.18f));
            Building(new Vector3(-13.5f, 0f, -2f), new Vector3(3f, 7f, 9f), new Color(0.15f, 0.1f, 0.2f), true, true);
            Building(new Vector3(13.5f, 0f, -1f), new Vector3(3f, 9f, 9f), new Color(0.14f, 0.11f, 0.22f), false, true);

            NeonSign(arena, "UNDERGROUND ARENA", new Vector3(-3.5f, 5.3f, -9.95f), new Color(3f, 0.5f, 1.9f), 150);
            NeonSign(arena, "WASH", new Vector3(-11f, 3.2f, -9.45f), new Color(0.5f, 2.6f, 3f), 110);
            NeonSign(arena, "DOPE", new Vector3(4.5f, 3.4f, -9.45f), new Color(0.6f, 3f, 0.9f), 110);
            NeonSign(arena, "CRISPR", new Vector3(12f, 4.6f, -8.95f), new Color(2.6f, 0.9f, 3f), 90);
        }

        private static void NeonSign(Node3D arena, string text, Vector3 position, Color hdrColor, int fontSize)
        {
            arena.AddChild(new Label3D
            {
                Text = text,
                FontSize = fontSize,
                PixelSize = 0.01f,
                Position = position,
                Modulate = hdrColor,
                OutlineSize = 0
            });
            arena.AddChild(new OmniLight3D
            {
                LightColor = new Color(hdrColor.R / 3f, hdrColor.G / 3f, hdrColor.B / 3f),
                LightEnergy = 1.6f,
                OmniRange = 6f,
                Position = position + new Vector3(0f, 0f, 1.2f)
            });
        }

        private static void BuildProps(Node3D arena)
        {
            Dumpster(arena, new Vector3(-7.2f, 0f, -4.5f), 20f, new Color(0.12f, 0.3f, 0.18f));
            Dumpster(arena, new Vector3(7.6f, 0f, -5.2f), -15f, new Color(0.3f, 0.2f, 0.1f));
            StreetLamp(arena, new Vector3(-7.5f, 0f, 2.2f), 0f);
            StreetLamp(arena, new Vector3(8f, 0f, -1.5f), 1.3f);

            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.DegToRad(200f + i * 28f);
                var position = new Vector3(Mathf.Cos(angle) * 6.2f, 0f, Mathf.Sin(angle) * 6.2f);
                Shapes.Toon(arena, Shapes.Cylinder(0.32f, 0.32f, 0.9f), position + new Vector3(0f, 0.45f, 0f), new Color(0.22f, 0.24f, 0.3f));
                if (i % 2 == 0)
                {
                    Shapes.Part(arena, Shapes.Sphere(0.18f), position + new Vector3(0f, 1.05f, 0f), MaterialFactory.Emissive(new Color(1f, 0.45f, 0.1f), 4f));
                    arena.AddChild(new OmniLight3D { LightColor = new Color(1f, 0.5f, 0.15f), LightEnergy = 1.2f, OmniRange = 3.5f, Position = position + new Vector3(0f, 1.3f, 0f) });
                }
            }
        }

        private static void Dumpster(Node3D arena, Vector3 position, float yaw, Color color)
        {
            var root = new Node3D { Position = position, RotationDegrees = new Vector3(0f, yaw, 0f) };
            arena.AddChild(root);
            Shapes.Toon(root, Shapes.Box(2.2f, 1.2f, 1.2f), new Vector3(0f, 0.6f, 0f), color, outline: 0.03f);
            Shapes.Toon(root, Shapes.Box(2.3f, 0.08f, 1.3f), new Vector3(0f, 1.25f, -0.15f), color.Darkened(0.4f), new Vector3(-18f, 0f, 0f), outline: 0.02f);
            Shapes.Toon(root, Shapes.Sphere(0.35f), new Vector3(1.4f, 0.3f, 0.4f), new Color(0.08f, 0.08f, 0.1f), scale: new Vector3(1f, 0.8f, 1f));
        }

        private static void StreetLamp(Node3D arena, Vector3 position, float flickerOffset)
        {
            var root = new Node3D { Position = position };
            arena.AddChild(root);
            Shapes.Toon(root, Shapes.Cylinder(0.06f, 0.09f, 5f), new Vector3(0f, 2.5f, 0f), new Color(0.15f, 0.15f, 0.18f));
            Shapes.Toon(root, Shapes.Box(0.9f, 0.12f, 0.3f), new Vector3(0.35f, 5f, 0f), new Color(0.15f, 0.15f, 0.18f));
            Shapes.Part(root, Shapes.Box(0.5f, 0.05f, 0.22f), new Vector3(0.5f, 4.92f, 0f), MaterialFactory.Emissive(new Color(1f, 0.75f, 0.4f), 5f));
            var light = new OmniLight3D { LightColor = new Color(1f, 0.7f, 0.4f), LightEnergy = 2.4f, OmniRange = 9f, Position = new Vector3(0.5f, 4.6f, 0f), ShadowEnabled = true };
            root.AddChild(light);

            var tween = root.CreateTween().SetLoops();
            tween.TweenInterval(2.4 + flickerOffset);
            tween.TweenProperty(light, "light_energy", 0.25f, 0.04);
            tween.TweenProperty(light, "light_energy", 2.4f, 0.05);
            tween.TweenProperty(light, "light_energy", 0.6f, 0.06).SetDelay(0.08);
            tween.TweenProperty(light, "light_energy", 2.4f, 0.08);
        }

        private static void BuildCrowd(Node3D arena)
        {
            var rng = new RandomNumberGenerator { Seed = 42 };
            for (int i = 0; i < 14; i++)
            {
                float angle = Mathf.DegToRad(195f + i * 11.5f + rng.RandfRange(-3f, 3f));
                float radius = rng.RandfRange(7.2f, 8.4f);
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var person = new Node3D { Position = position, Scale = Vector3.One * rng.RandfRange(0.85f, 1.05f) };
                arena.AddChild(person);
                var shade = new Color(0.08f, 0.06f, 0.12f).Lerp(new Color(0.2f, 0.12f, 0.25f), rng.Randf());
                Shapes.Toon(person, Shapes.Capsule(0.28f, 1.5f), new Vector3(0f, 0.75f, 0f), shade, outline: 0.015f);
                Shapes.Toon(person, Shapes.Sphere(0.2f), new Vector3(0f, 1.68f, 0f), shade.Lightened(0.1f), outline: 0.015f);
                if (rng.Randf() < 0.45f)
                    Shapes.Part(person, Shapes.Box(0.1f, 0.16f, 0.02f), new Vector3(0.18f, 1.75f, 0.2f), MaterialFactory.Emissive(Palette.NeonCyan.Lerp(Colors.White, 0.4f), 3f));
                person.LookAt(ArenaCenter + Vector3.Up * 0.8f, Vector3.Up, true);
            }
        }

        private static Node3D Trainer(Node3D arena, Vector3 position, Color outfit, Color accent, string tag)
        {
            var trainer = new Node3D { Name = $"Trainer_{tag}", Position = position };
            arena.AddChild(trainer);
            Shapes.Humanoid(trainer, outfit, new Color(0.82f, 0.62f, 0.48f), accent);
            trainer.AddChild(new Label3D
            {
                Text = tag,
                FontSize = 48,
                PixelSize = 0.006f,
                Position = new Vector3(0f, 2.3f, 0f),
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                Modulate = accent,
                OutlineSize = 10,
                OutlineModulate = new Color(0.02f, 0f, 0.05f)
            });
            trainer.LookAt(ArenaCenter with { Y = 0f }, Vector3.Up, true);
            return trainer;
        }
    }
}
