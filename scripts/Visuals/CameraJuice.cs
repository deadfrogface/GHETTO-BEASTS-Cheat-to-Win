using System.Threading.Tasks;
using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>
    /// Combat / UI juice: directional shake, hit-pause, dramatic framing,
    /// speedlines + inverted KO flash overlays.
    /// </summary>
    public static class CameraJuice
    {
        public static void HitStop(Node host, double duration = 0.06, float scale = 0.15f)
        {
            // Map legacy API onto TimeScaleManager (light ≈ 0.05, heavy ≈ 0.12).
            bool heavy = duration >= 0.1;
            TimeScaleManager.HitPause(host, heavy, scale);
        }

        public static async Task HitStopAsync(Node host, double duration = 0.06, float scale = 0.15f)
        {
            HitStop(host, duration, scale);
            await Vfx.Delay(host, duration);
            TimeScaleManager.Reset();
        }

        /// <summary>Directional screen shake along a world-space impulse (projected to camera offsets).</summary>
        public static void DirectionalShake(Camera3D camera, Vector3 worldImpulse, float intensity = 0.2f, double duration = 0.28)
        {
            if (camera is null) return;
            Vector3 local = camera.GlobalTransform.Basis.Inverse() * worldImpulse;
            if (local.LengthSquared() < 0.0001f)
                local = new Vector3(1f, 0.4f, 0f);
            local = local.Normalized();

            float hBias = local.X;
            float vBias = local.Y * 0.65f + local.Z * 0.35f;

            var tween = camera.CreateTween();
            tween.TweenMethod(Callable.From<float>(remaining =>
            {
                float strength = intensity * remaining;
                float jitterH = (float)GD.RandRange(-strength * 0.35, strength * 0.35);
                float jitterV = (float)GD.RandRange(-strength * 0.35, strength * 0.35);
                camera.HOffset = hBias * strength + jitterH;
                camera.VOffset = vBias * strength + jitterV;
            }), 1f, 0f, duration);
            tween.TweenCallback(Callable.From(() =>
            {
                camera.HOffset = 0f;
                camera.VOffset = 0f;
            }));
        }

        public static void DramaticAction(Camera3D camera, Vector3 lookAt, float fovPunch = -10f, double duration = 0.45)
        {
            if (camera is null) return;
            float baseFov = camera.Fov;
            var tween = camera.CreateTween().SetParallel();
            tween.TweenProperty(camera, "fov", baseFov + fovPunch, duration * 0.35)
                .SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
            tween.Chain().TweenProperty(camera, "fov", baseFov, duration * 0.65)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            if (camera.GlobalPosition.DistanceSquaredTo(lookAt) > 0.01f)
                camera.LookAt(lookAt, Vector3.Up);
        }

        public static ColorRect AttachSpeedlines(CanvasLayer layer, Color? tint = null)
        {
            var shader = GD.Load<Shader>("res://shaders/speedlines.gdshader");
            var mat = new ShaderMaterial { Shader = shader };
            mat.SetShaderParameter("intensity", 0f);
            mat.SetShaderParameter("line_color", tint ?? new Color(1f, 1f, 1f, 0.55f));
            var rect = new ColorRect
            {
                Name = "Speedlines",
                Material = mat,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Color = Colors.White
            };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            layer.AddChild(rect);
            return rect;
        }

        public static void PulseSpeedlines(ColorRect? overlay, float peak = 0.85f, double duration = 0.35)
        {
            if (overlay?.Material is not ShaderMaterial mat) return;
            var tween = overlay.CreateTween();
            tween.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("intensity", v)), 0f, peak, duration * 0.25);
            tween.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("intensity", v)), peak, 0f, duration * 0.75);
        }

        public static ColorRect AttachInvertedImpact(CanvasLayer layer)
        {
            var shader = GD.Load<Shader>("res://shaders/inverted_impact.gdshader");
            var mat = new ShaderMaterial { Shader = shader };
            mat.SetShaderParameter("intensity", 0f);
            var rect = new ColorRect
            {
                Name = "InvertedImpact",
                Material = mat,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Color = Colors.White
            };
            rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            layer.AddChild(rect);
            return rect;
        }

        /// <summary>KO flash: inverted-impact ring + brief heavy hit-pause.</summary>
        public static void PulseKoFlash(Node host, ColorRect? overlay, Color? tint = null)
        {
            TimeScaleManager.HitPause(host, heavy: true);
            if (overlay?.Material is not ShaderMaterial mat) return;
            if (tint.HasValue) mat.SetShaderParameter("flash_color", tint.Value);
            var tween = overlay.CreateTween();
            tween.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("intensity", v)), 0f, 1.15f, 0.08);
            tween.TweenMethod(Callable.From<float>(v => mat.SetShaderParameter("intensity", v)), 1.15f, 0f, 0.45);
        }
    }
}
