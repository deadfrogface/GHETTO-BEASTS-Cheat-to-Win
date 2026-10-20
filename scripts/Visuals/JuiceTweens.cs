using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>
    /// Squash / stretch / punch scale for procedural beasts that lack Mixamo clips.
    /// </summary>
    public static class JuiceTweens
    {
        public static void SquashStretch(Node3D target, float baseScale = 1f, bool heavy = false)
        {
            if (target is null) return;
            float squashY = heavy ? 0.62f : 0.78f;
            float stretchY = heavy ? 1.28f : 1.14f;
            float squashXz = heavy ? 1.28f : 1.14f;
            float stretchXz = heavy ? 0.82f : 0.9f;

            var tween = target.CreateTween();
            tween.TweenProperty(target, "scale", new Vector3(baseScale * squashXz, baseScale * squashY, baseScale * squashXz), 0.06)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(target, "scale", new Vector3(baseScale * stretchXz, baseScale * stretchY, baseScale * stretchXz), 0.08)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(target, "scale", Vector3.One * baseScale, 0.14)
                .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        }

        public static void AttackWindup(Node3D target, float baseScale = 1f)
        {
            if (target is null) return;
            var tween = target.CreateTween();
            tween.TweenProperty(target, "scale", new Vector3(baseScale * 1.08f, baseScale * 0.88f, baseScale * 1.08f), 0.08);
            tween.TweenProperty(target, "scale", Vector3.One * baseScale, 0.12)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }

        public static void HealPulse(Node3D target, float baseScale = 1f)
        {
            if (target is null) return;
            var tween = target.CreateTween();
            tween.TweenProperty(target, "scale", Vector3.One * (baseScale * 1.12f), 0.15)
                .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(target, "scale", Vector3.One * baseScale, 0.25)
                .SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        }

        public static void FaintCollapse(Node3D target, float baseScale = 1f)
        {
            if (target is null) return;
            var tween = target.CreateTween();
            tween.TweenProperty(target, "scale", new Vector3(baseScale * 1.2f, baseScale * 0.35f, baseScale * 1.2f), 0.35)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        }
    }
}
