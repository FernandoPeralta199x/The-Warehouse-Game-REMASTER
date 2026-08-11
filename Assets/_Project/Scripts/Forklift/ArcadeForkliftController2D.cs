using System;
using System.Collections;
using TW08.Core;
using TW08.Input;
using UnityEngine;

namespace TW08.Race
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public sealed class ArcadeForkliftController2D : MonoBehaviour
    {
        [SerializeField] private ForkliftStats stats;
        [SerializeField] private GameInput input;
        [SerializeField] private bool playerControlled = true;
        [SerializeField, Min(0f)] private float collisionDamageScale = 4f;

        private Rigidbody2D body;
        private Coroutine speedModifierRoutine;
        private Coroutine gripAssistRoutine;
        private Coroutine handlingAssistRoutine;
        private Coroutine impactProtectionRoutine;
        private float speedMultiplier = 1f;
        private float surfaceGripMultiplier = 1f;
        private float temporaryGripMultiplier = 1f;
        private float steeringMultiplier = 1f;
        private float impactDamageMultiplier = 1f;
        private float driftCharge;
        private bool driftWasHeld;
        private float aiSteer;
        private float aiThrottle;
        private bool aiDrift;

        public float CurrentSpeed => body == null ? 0f : body.linearVelocity.magnitude;
        public float NormalizedSpeed => stats == null ? 0f : Mathf.Clamp01(CurrentSpeed / Mathf.Max(0.01f, stats.MaxForwardSpeed));
        public bool IsDrifting { get; private set; }
        public bool ControlsEnabled { get; set; } = true;
        public bool PlayerControlled => playerControlled;
        public Rigidbody2D Body => body;

        public event Action<float> Impacted;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
        }

        private void OnEnable()
        {
            if (playerControlled && input != null)
            {
                input.SetMode(GameMode.Race);
            }
        }

        public void Configure(GameInput gameInput, ForkliftStats forkliftStats, bool isPlayer)
        {
            input = gameInput;
            stats = forkliftStats;
            playerControlled = isPlayer;
        }

        public void SetAiInput(float steer, float throttle, bool drift)
        {
            aiSteer = Mathf.Clamp(steer, -1f, 1f);
            aiThrottle = Mathf.Clamp(throttle, -1f, 1f);
            aiDrift = drift;
        }

        public void SetSurfaceGripMultiplier(float multiplier)
        {
            surfaceGripMultiplier = Mathf.Clamp(multiplier, 0.15f, 2f);
        }

        public void ApplyBoost(float multiplier, float duration)
        {
            ApplyTimedSpeedMultiplier(Mathf.Max(1f, multiplier), duration);
        }

        public void ApplySlow(float multiplier, float duration)
        {
            ApplyTimedSpeedMultiplier(Mathf.Clamp(multiplier, 0.2f, 1f), duration);
        }

        public void ApplyTimedSpeedMultiplier(float multiplier, float duration)
        {
            if (speedModifierRoutine != null)
            {
                StopCoroutine(speedModifierRoutine);
            }

            speedModifierRoutine = StartCoroutine(SpeedModifierRoutine(multiplier, duration));
        }

        public void ApplyGripAssist(float multiplier, float duration)
        {
            if (gripAssistRoutine != null)
            {
                StopCoroutine(gripAssistRoutine);
            }
            gripAssistRoutine = StartCoroutine(GripAssistRoutine(Mathf.Clamp(multiplier, 0.25f, 3f), duration));
        }

        public void ApplyHandlingAssist(float multiplier, float duration)
        {
            if (handlingAssistRoutine != null)
            {
                StopCoroutine(handlingAssistRoutine);
            }
            handlingAssistRoutine = StartCoroutine(HandlingAssistRoutine(Mathf.Clamp(multiplier, 0.5f, 2f), duration));
        }

        public void ApplyImpactProtection(float damageMultiplier, float duration)
        {
            if (impactProtectionRoutine != null)
            {
                StopCoroutine(impactProtectionRoutine);
            }
            impactProtectionRoutine = StartCoroutine(ImpactProtectionRoutine(Mathf.Clamp(damageMultiplier, 0f, 1f), duration));
        }

        private void FixedUpdate()
        {
            if (stats == null || !ControlsEnabled)
            {
                return;
            }

            float steer = playerControlled && input != null ? input.RaceSteer : aiSteer;
            float throttle = playerControlled && input != null ? input.RaceThrottle : aiThrottle;
            bool driftHeld = playerControlled && input != null ? input.RaceDriftHeld : aiDrift;

            ApplyDrive(throttle);
            ApplySteering(steer);
            ApplyLateralGrip(driftHeld);
            UpdateDriftBoost(driftHeld);
            ClampVelocity(throttle);
        }

        private void ApplyDrive(float throttle)
        {
            Vector2 forward = transform.up;
            float forwardSpeed = Vector2.Dot(body.linearVelocity, forward);
            float speedLimit = (throttle >= 0f ? stats.MaxForwardSpeed : stats.MaxReverseSpeed) * speedMultiplier;

            if (Mathf.Abs(forwardSpeed) < speedLimit)
            {
                float force = throttle >= 0f ? stats.Acceleration : stats.BrakeForce;
                body.AddForce(forward * (throttle * force), ForceMode2D.Force);
            }

            if (Mathf.Abs(throttle) < 0.01f)
            {
                body.linearVelocity *= Mathf.Clamp01(1f - stats.RollingResistance);
            }
        }

        private void ApplySteering(float steer)
        {
            float forwardSpeed = Vector2.Dot(body.linearVelocity, transform.up);
            float directionSign = Mathf.Abs(forwardSpeed) < 0.05f ? 1f : Mathf.Sign(forwardSpeed);
            float speedFactor = Mathf.Lerp(stats.LowSpeedSteering, 1f, NormalizedSpeed);
            float rotation = steer * stats.SteeringDegreesPerSecond * steeringMultiplier * speedFactor * directionSign * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation - rotation);
        }

        private void ApplyLateralGrip(bool driftHeld)
        {
            Vector2 forward = transform.up;
            Vector2 right = transform.right;
            Vector2 forwardVelocity = forward * Vector2.Dot(body.linearVelocity, forward);
            Vector2 lateralVelocity = right * Vector2.Dot(body.linearVelocity, right);
            float retention = driftHeld ? stats.DriftLateralRetention : stats.NormalLateralRetention;
            retention = Mathf.Clamp01(retention * surfaceGripMultiplier * temporaryGripMultiplier);
            body.linearVelocity = forwardVelocity + lateralVelocity * retention;
            IsDrifting = driftHeld && NormalizedSpeed > 0.25f;
        }

        private void UpdateDriftBoost(bool driftHeld)
        {
            if (IsDrifting)
            {
                driftCharge = Mathf.Min(stats.MaximumDriftCharge, driftCharge + Time.fixedDeltaTime);
            }

            if (driftWasHeld && !driftHeld)
            {
                if (driftCharge >= stats.MinimumDriftCharge)
                {
                    float chargeRatio = Mathf.InverseLerp(stats.MinimumDriftCharge, stats.MaximumDriftCharge, driftCharge);
                    float duration = stats.DriftBoostDuration * Mathf.Lerp(0.65f, 1.35f, chargeRatio);
                    ApplyBoost(stats.DriftBoostMultiplier, duration);
                }

                driftCharge = 0f;
            }

            driftWasHeld = driftHeld;
        }

        private void ClampVelocity(float throttle)
        {
            float max = (throttle < 0f ? stats.MaxReverseSpeed : stats.MaxForwardSpeed) * speedMultiplier * 1.2f;
            if (body.linearVelocity.magnitude > max)
            {
                body.linearVelocity = body.linearVelocity.normalized * max;
            }
        }

        private IEnumerator SpeedModifierRoutine(float multiplier, float duration)
        {
            speedMultiplier = multiplier;
            yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
            speedMultiplier = 1f;
            speedModifierRoutine = null;
        }

        private IEnumerator GripAssistRoutine(float multiplier, float duration)
        {
            temporaryGripMultiplier = multiplier;
            yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
            temporaryGripMultiplier = 1f;
            gripAssistRoutine = null;
        }

        private IEnumerator HandlingAssistRoutine(float multiplier, float duration)
        {
            steeringMultiplier = multiplier;
            yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
            steeringMultiplier = 1f;
            handlingAssistRoutine = null;
        }

        private IEnumerator ImpactProtectionRoutine(float multiplier, float duration)
        {
            impactDamageMultiplier = multiplier;
            yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
            impactDamageMultiplier = 1f;
            impactProtectionRoutine = null;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact <= 0f)
            {
                return;
            }

            Impacted?.Invoke(impact);

            ForkliftDamage damage = GetComponent<ForkliftDamage>();
            if (damage == null || impact <= 2f)
            {
                return;
            }

            damage.ApplyDamage((impact - 2f) * collisionDamageScale * impactDamageMultiplier);
        }
    }
}
