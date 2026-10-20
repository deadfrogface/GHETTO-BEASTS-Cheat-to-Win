using Godot;

namespace GhettoBeasts.Visuals
{
    public partial class CameraRig : Camera3D
    {
        public const float CombatFov = 60f;

        private float baseFov = CombatFov;
        private Tween? shakeTween;
        private Tween? punchTween;

        public override void _Ready()
        {
            baseFov = Fov > 1f ? Fov : CombatFov;
        }

        /// <summary>
        /// Pokémon / Palworld combat framing: deep behind the player trainer &amp; active beast,
        /// slight upward look toward the enemy, FOV 60.
        /// </summary>
        public void PlaceCombatCamera(Vector3 playerSpot, Vector3 enemySpot, Vector3? playerTrainerPos = null)
        {
            Vector3 toEnemy = (enemySpot - playerSpot) with { Y = 0f };
            if (toEnemy.LengthSquared() < 0.001f) toEnemy = new Vector3(1f, 0f, -1f);
            toEnemy = toEnemy.Normalized();

            Vector3 anchor = playerTrainerPos ?? (playerSpot - toEnemy * 1.8f);
            Vector3 camPos = anchor - toEnemy * 4.6f + Vector3.Up * 2.55f + toEnemy.Cross(Vector3.Up).Normalized() * -0.35f;
            Vector3 lookAt = enemySpot.Lerp(playerSpot, 0.15f) + Vector3.Up * 1.15f;

            Fov = CombatFov;
            baseFov = CombatFov;
            GlobalPosition = camPos;
            LookAt(lookAt, Vector3.Up);
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
