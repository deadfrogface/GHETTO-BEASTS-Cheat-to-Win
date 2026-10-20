using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>
    /// Brief combat hit-pause: drop <see cref="Engine.TimeScale"/>, hold, then tween back to 1.
    /// Light = 0.05s, heavy = 0.12s (KO / crit / card slam).
    /// </summary>
    public static class TimeScaleManager
    {
        public const double LightDuration = 0.05;
        public const double HeavyDuration = 0.12;
        public const float DefaultScale = 0.12f;

        private static Tween? activeTween;

        public static void HitPause(Node host, bool heavy = false, float scale = DefaultScale)
        {
            if (host is null || DisplayServer.GetName() == "headless") return;
            if (host.GetTree() is null) return;

            double hold = heavy ? HeavyDuration : LightDuration;
            float clamped = Mathf.Clamp(scale, 0.05f, 0.45f);

            activeTween?.Kill();
            Engine.TimeScale = clamped;

            // Interval runs in scaled time; compensate so wall-ish hold ≈ Light/HeavyDuration.
            double scaledHold = hold * Mathf.Max(clamped, 0.05f);
            activeTween = host.CreateTween();
            activeTween.TweenInterval(scaledHold);
            activeTween.TweenMethod(
                Callable.From<float>(v => Engine.TimeScale = v),
                clamped,
                1f,
                heavy ? 0.18 : 0.1);
            activeTween.TweenCallback(Callable.From(() => Engine.TimeScale = 1f));
        }

        public static void Reset()
        {
            activeTween?.Kill();
            activeTween = null;
            Engine.TimeScale = 1f;
        }
    }
}
