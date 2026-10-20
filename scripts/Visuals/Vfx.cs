using System;
using System.Threading.Tasks;
using Godot;

namespace GhettoBeasts.Visuals
{
    public static class Vfx
    {
        public static void Burst(Node parent, Vector3 position, Color color, int amount = 70, float speed = 5f,
            float lifetime = 0.9f, float size = 0.05f, float gravity = -7f, float spread = 180f)
        {
            if (parent is null) return;
            // Headless/dummy renderer: skip mesh particles (mesh_get_surface_count null).
            if (DisplayServer.GetName() == "headless")
            {
                Flash(parent, position, color, 4f, 0.25f, 3f);
                return;
            }

            var particles = new CpuParticles3D
            {
                OneShot = true,
                Emitting = false,
                Amount = amount,
                Lifetime = lifetime,
                Explosiveness = 0.95f,
                Mesh = new SphereMesh { Radius = size, Height = size * 2f, RadialSegments = 6, Rings = 3, Material = MaterialFactory.Emissive(color, 4f) },
                Direction = Vector3.Up,
                Spread = spread,
                InitialVelocityMin = speed * 0.35f,
                InitialVelocityMax = speed,
                Gravity = new Vector3(0f, gravity, 0f),
                ScaleAmountMin = 0.4f,
                ScaleAmountMax = 1.4f,
                DampingMin = 1f,
                DampingMax = 3f
            };
            parent.AddChild(particles);
            particles.GlobalPosition = position;
            particles.Emitting = true;
            particles.Finished += particles.QueueFree;
        }

        public static void Smoke(Node parent, Vector3 position)
        {
            if (parent is null || DisplayServer.GetName() == "headless") return;
            var smoke = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.55f, 0.5f, 0.65f, 0.55f),
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            };
            var particles = new CpuParticles3D
            {
                OneShot = true,
                Emitting = false,
                Amount = 24,
                Lifetime = 1.2f,
                Explosiveness = 0.9f,
                Mesh = new SphereMesh { Radius = 0.25f, Height = 0.5f, RadialSegments = 8, Rings = 4, Material = smoke },
                Direction = Vector3.Up,
                Spread = 85f,
                Flatness = 0.7f,
                InitialVelocityMin = 1.5f,
                InitialVelocityMax = 3.5f,
                Gravity = new Vector3(0f, 0.6f, 0f),
                DampingMin = 2f,
                DampingMax = 4f,
                ScaleAmountMin = 0.6f,
                ScaleAmountMax = 1.8f
            };
            parent.AddChild(particles);
            particles.GlobalPosition = position + Vector3.Up * 0.1f;
            particles.Emitting = true;
            particles.Finished += particles.QueueFree;
        }

        public static void Flash(Node parent, Vector3 position, Color color, float energy = 8f, float duration = 0.4f, float range = 8f)
        {
            var light = new OmniLight3D { LightColor = color, LightEnergy = energy, OmniRange = range };
            parent.AddChild(light);
            light.GlobalPosition = position;
            var tween = light.CreateTween();
            tween.TweenProperty(light, "light_energy", 0f, duration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenCallback(Callable.From(light.QueueFree));
        }

        public static void FloatingText(Node parent, Vector3 position, string text, Color color, int fontSize = 64) =>
            FloatingCombatText(parent, position, text, color, fontSize);

        /// <summary>Floating combat text (FCT) with slight punch scale.</summary>
        public static void FloatingCombatText(Node parent, Vector3 position, string text, Color color, int fontSize = 64)
        {
            var label = new Label3D
            {
                Text = text,
                FontSize = fontSize,
                OutlineSize = 14,
                OutlineModulate = new Color(0.03f, 0f, 0.06f),
                Modulate = color,
                Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
                NoDepthTest = true,
                PixelSize = 0.006f,
                Scale = Vector3.One * 0.35f
            };
            parent.AddChild(label);
            label.GlobalPosition = position;

            var tween = label.CreateTween().SetParallel();
            tween.TweenProperty(label, "scale", Vector3.One, 0.12).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(label, "global_position", position + new Vector3((float)GD.RandRange(-0.3, 0.3), 1.1f, 0f), 1.0)
                .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(label, "modulate:a", 0f, 0.5).SetDelay(0.55);
            tween.TweenProperty(label, "outline_modulate:a", 0f, 0.5).SetDelay(0.55);
            tween.Chain().TweenCallback(Callable.From(label.QueueFree));
        }

        /// <summary>Expanding ring shockwave used on card slam / heavy hits (+ radial particle burst).</summary>
        public static void Shockwave(Node parent, Vector3 position, Color color, float maxRadius = 3.2f, double duration = 0.35)
        {
            if (parent is null) return;
            var ring = new MeshInstance3D
            {
                Mesh = new TorusMesh { InnerRadius = 0.05f, OuterRadius = 0.12f, Rings = 12, RingSegments = 24 },
                MaterialOverride = MaterialFactory.Emissive(color, 3.5f),
                Scale = Vector3.One * 0.2f
            };
            parent.AddChild(ring);
            ring.GlobalPosition = position + Vector3.Up * 0.05f;
            ring.RotationDegrees = new Vector3(90f, 0f, 0f);
            var tween = ring.CreateTween().SetParallel();
            tween.TweenProperty(ring, "scale", Vector3.One * maxRadius, duration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(ring, "transparency", 1f, duration);
            tween.Chain().TweenCallback(Callable.From(ring.QueueFree));
            Flash(parent, position + Vector3.Up * 0.4f, color, 6f, (float)duration, maxRadius * 1.5f);

            // Radial ground dust / spark ring for card slam impact.
            if (DisplayServer.GetName() != "headless")
            {
                var sparks = new CpuParticles3D
                {
                    OneShot = true,
                    Emitting = false,
                    Amount = 48,
                    Lifetime = duration * 0.9,
                    Explosiveness = 1f,
                    Mesh = new SphereMesh { Radius = 0.04f, Height = 0.08f, RadialSegments = 4, Rings = 2, Material = MaterialFactory.Emissive(color, 4f) },
                    Direction = Vector3.Up,
                    Spread = 180f,
                    Flatness = 0.85f,
                    InitialVelocityMin = maxRadius * 1.2f,
                    InitialVelocityMax = maxRadius * 2.4f,
                    Gravity = new Vector3(0f, -2f, 0f),
                    ScaleAmountMin = 0.5f,
                    ScaleAmountMax = 1.3f,
                    DampingMin = 3f,
                    DampingMax = 6f
                };
                parent.AddChild(sparks);
                sparks.GlobalPosition = position + Vector3.Up * 0.08f;
                sparks.Emitting = true;
                sparks.Finished += sparks.QueueFree;
            }
        }

        /// <summary>Moves <paramref name="node"/> along a parabolic arc and awaits arrival.</summary>
        public static async Task ArcAsync(Node3D node, Vector3 from, Vector3 to, float height, double duration, float spin = 0f)
        {
            var tween = node.CreateTween();
            tween.TweenMethod(Callable.From<float>(t =>
            {
                var position = from.Lerp(to, t);
                position.Y += 4f * height * t * (1f - t);
                node.GlobalPosition = position;
                if (spin != 0f) node.RotateObjectLocal(Vector3.Right, spin * 0.016f);
            }), 0f, 1f, duration);
            await node.ToSignal(tween, Tween.SignalName.Finished);
        }

        public static async Task ThrowAsync(Node parent, Mesh mesh, Material material, Vector3 from, Vector3 to,
            float height = 1.5f, double duration = 0.55, float spin = 12f)
        {
            var projectile = new MeshInstance3D { Mesh = mesh, MaterialOverride = material };
            parent.AddChild(projectile);
            projectile.GlobalPosition = from;
            await ArcAsync(projectile, from, to, height, duration, spin);
            projectile.QueueFree();
        }

        /// <summary>
        /// Tweens a shader uniform via <see cref="ShaderMaterial.SetShaderParameter"/>. Unlike
        /// <c>TweenProperty("shader_parameter/x")</c> this does not depend on the renderer exposing the
        /// uniform list (the headless dummy renderer does not), so it never returns a null tweener.
        /// </summary>
        public static MethodTweener TweenShaderParameter(this Tween tween, ShaderMaterial material, string parameter,
            float to, double duration, float? from = null)
        {
            float start = from ?? material.GetShaderParameter(parameter).AsSingle();
            return tween.TweenMethod(Callable.From<float>(v => material.SetShaderParameter(parameter, v)), start, to, duration);
        }

        public static async Task Delay(Node node, double seconds)
        {
            await node.ToSignal(node.GetTree().CreateTimer(Math.Max(0.0, seconds)), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
