using Godot;

namespace GhettoBeasts.Visuals
{
    /// <summary>
    /// Smooth follow camera (C# stand-in for Phantom Camera): third-person district chase,
    /// over-shoulder combat framing, brief action cuts.
    /// </summary>
    public partial class CameraRig : Camera3D
    {
        public const float CombatFov = 60f;
        public const float DistrictFov = 48f;

        public enum FollowMode
        {
            Manual,
            District,
            Combat,
            ActionCut
        }

        private float baseFov = CombatFov;
        private Tween? shakeTween;
        private Tween? punchTween;
        private Tween? cutTween;

        private FollowMode mode = FollowMode.Manual;
        private Node3D? followTarget;
        private Vector3 lookAtPoint;
        private Vector3 combatPlayerSpot;
        private Vector3 combatEnemySpot;
        private Vector3 combatTrainerPos;
        private float districtYaw;
        private float districtDistance = 7.2f;
        private float districtHeight = 4.2f;
        private float followLerp = 4.5f;
        private double actionCutRemaining;
        private Vector3 actionCutLook;
        private Vector3 actionCutFrom;
        private float actionCutFov;

        public FollowMode Mode => mode;

        public override void _Ready()
        {
            baseFov = Fov > 1f ? Fov : CombatFov;
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            if (mode == FollowMode.Manual) return;
            float dt = (float)delta;

            if (mode == FollowMode.ActionCut)
            {
                actionCutRemaining -= delta;
                GlobalPosition = GlobalPosition.Lerp(actionCutFrom, 1f - Mathf.Exp(-10f * dt));
                if (GlobalPosition.DistanceSquaredTo(actionCutLook) > 0.01f)
                    LookAt(actionCutLook, Vector3.Up);
                Fov = Mathf.Lerp(Fov, actionCutFov, 1f - Mathf.Exp(-8f * dt));
                if (actionCutRemaining <= 0)
                    mode = FollowMode.Combat;
                return;
            }

            if (mode == FollowMode.District && followTarget is not null)
            {
                Vector3 back = Basis.FromEuler(new Vector3(0f, districtYaw, 0f)) * Vector3.Back;
                Vector3 target = followTarget.GlobalPosition + back * districtDistance + Vector3.Up * districtHeight;
                GlobalPosition = GlobalPosition.Lerp(target, 1f - Mathf.Exp(-followLerp * dt));
                Vector3 look = followTarget.GlobalPosition + Vector3.Up * 1.2f;
                LookAt(look, Vector3.Up);
                return;
            }

            if (mode == FollowMode.Combat)
                ApplyCombatFraming(smooth: true, dt);
        }

        public void BeginDistrictFollow(Node3D target, float yaw = 0f)
        {
            followTarget = target;
            districtYaw = yaw;
            mode = FollowMode.District;
            Fov = DistrictFov;
            baseFov = DistrictFov;
            Current = true;
        }

        public void SetDistrictYaw(float yaw) => districtYaw = yaw;

        public void BeginCombatFollow(Vector3 playerSpot, Vector3 enemySpot, Vector3? playerTrainerPos = null)
        {
            combatPlayerSpot = playerSpot;
            combatEnemySpot = enemySpot;
            combatTrainerPos = playerTrainerPos ?? (playerSpot - Vector3.Forward * 1.8f);
            mode = FollowMode.Combat;
            PlaceCombatCamera(playerSpot, enemySpot, playerTrainerPos);
        }

        /// <summary>
        /// Pokémon / Palworld combat framing: deep behind the player trainer &amp; active beast,
        /// slight upward look toward the enemy, FOV 60 (over-shoulder).
        /// </summary>
        public void PlaceCombatCamera(Vector3 playerSpot, Vector3 enemySpot, Vector3? playerTrainerPos = null)
        {
            combatPlayerSpot = playerSpot;
            combatEnemySpot = enemySpot;
            combatTrainerPos = playerTrainerPos ?? (playerSpot - Vector3.Forward * 1.8f);
            ApplyCombatFraming(smooth: false, 1f);
            mode = FollowMode.Combat;
        }

        private void ApplyCombatFraming(bool smooth, float dt)
        {
            Vector3 toEnemy = (combatEnemySpot - combatPlayerSpot) with { Y = 0f };
            if (toEnemy.LengthSquared() < 0.001f) toEnemy = new Vector3(1f, 0f, -1f);
            toEnemy = toEnemy.Normalized();

            Vector3 anchor = combatTrainerPos;
            // Over-shoulder: slightly right of trainer, looking past beast toward foe.
            Vector3 right = toEnemy.Cross(Vector3.Up).Normalized();
            Vector3 camPos = anchor - toEnemy * 4.4f + Vector3.Up * 2.45f + right * 0.85f;
            Vector3 lookAt = combatEnemySpot.Lerp(combatPlayerSpot, 0.12f) + Vector3.Up * 1.2f;

            Fov = CombatFov;
            baseFov = CombatFov;
            if (smooth)
            {
                GlobalPosition = GlobalPosition.Lerp(camPos, 1f - Mathf.Exp(-5.5f * dt));
                lookAtPoint = lookAtPoint.Lerp(lookAt, 1f - Mathf.Exp(-6f * dt));
                LookAt(lookAtPoint, Vector3.Up);
            }
            else
            {
                GlobalPosition = camPos;
                lookAtPoint = lookAt;
                LookAt(lookAt, Vector3.Up);
            }
        }

        /// <summary>Brief cinematic cut toward an impact point, then return to combat framing.</summary>
        public void ActionCut(Vector3 lookAt, float fovPunch = -10f, double duration = 0.4)
        {
            Vector3 to = (lookAt - GlobalPosition).Normalized();
            actionCutFrom = GlobalPosition + to * 0.85f + Vector3.Up * 0.15f;
            actionCutLook = lookAt;
            actionCutFov = CombatFov + fovPunch;
            actionCutRemaining = duration;
            mode = FollowMode.ActionCut;
            cutTween?.Kill();
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

        public void DirectionalShake(Vector3 worldImpulse, float intensity = 0.2f, double duration = 0.28) =>
            CameraJuice.DirectionalShake(this, worldImpulse, intensity, duration);

        public void Punch(float fovDelta = -6f, double duration = 0.3)
        {
            punchTween?.Kill();
            Fov = baseFov + fovDelta;
            punchTween = CreateTween();
            punchTween.TweenProperty(this, "fov", baseFov, duration).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        }
    }
}
