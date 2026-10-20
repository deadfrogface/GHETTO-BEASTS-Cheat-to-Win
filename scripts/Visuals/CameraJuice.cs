using System.Threading.Tasks;
using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>Combat / UI juice: hit-stop, dramatic framing, speedline overlay helpers.</summary>
    public static class CameraJuice
    {
        private static Tween? hitStopTween;

        public static void HitStop(Node host, double duration = 0.06, float scale = 0.15f)
        {
            if (host is null || DisplayServer.GetName() == "headless") return;
            var tree = host.GetTree();
            if (tree is null) return;
            hitStopTween?.Kill();
            Engine.TimeScale = Mathf.Clamp(scale, 0.05f, 1f);
            hitStopTween = host.CreateTween();
            hitStopTween.TweenInterval(duration * scale); // wall-ish: duration is in scaled time
            hitStopTween.TweenCallback(Callable.From(() => Engine.TimeScale = 1f));
        }

        public static async Task HitStopAsync(Node host, double duration = 0.06, float scale = 0.15f)
        {
            HitStop(host, duration, scale);
            await Vfx.Delay(host, duration);
            Engine.TimeScale = 1f;
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
    }
}
