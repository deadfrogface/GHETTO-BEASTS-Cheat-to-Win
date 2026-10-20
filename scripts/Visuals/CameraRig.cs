using Godot;

namespace GhettoBeasts.Visuals
{
    public partial class CameraRig : Camera3D
    {
        private float baseFov;
        private Tween? shakeTween;
        private Tween? punchTween;

        public override void _Ready()
        {
            baseFov = Fov;
        }

        public void Shake(float intensity = 0.15f, double duration = 0.35)
        {
            shakeTween?.Kill();
            shakeTween = CreateTween();
            shakeTween.TweenMethod(Callable.From<float>(remaining =>
            {
                float strength = intensity * remaining;
                HOffset = (float)GD.RandRange(-strength, strength);
                VOffset = (float)GD.RandRange(-strength, strength);
            }), 1f, 0f, duration);
            shakeTween.TweenCallback(Callable.From(() =>
            {
                HOffset = 0f;
                VOffset = 0f;
            }));
        }

        public void Punch(float fovDelta = -6f, double duration = 0.3)
        {
            punchTween?.Kill();
            Fov = baseFov + fovDelta;
            punchTween = CreateTween();
            punchTween.TweenProperty(this, "fov", baseFov, duration).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }
    }
}
