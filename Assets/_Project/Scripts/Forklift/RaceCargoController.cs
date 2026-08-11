using System;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceCargoController : MonoBehaviour
    {
        [SerializeField] private ArcadeForkliftController2D vehicle;
        [SerializeField, Min(1f)] private float maximumIntegrity = 100f;
        [SerializeField, Min(0f)] private float lateralStressThreshold = 2.2f;
        [SerializeField, Min(0f)] private float lateralDamagePerSecond = 2.8f;
        [SerializeField, Min(0f)] private float impactDamageScale = 1.6f;
        [SerializeField, Range(0.1f, 2f)] private float stabilityMultiplier = 1f;

        private Rigidbody2D body;
        private float protectionMultiplier = 1f;
        private float protectionUntil;

        public float Integrity { get; private set; }
        public float MaximumIntegrity => maximumIntegrity;
        public float DamagePercent => maximumIntegrity <= 0f
            ? 0f
            : Mathf.Clamp01(1f - Integrity / maximumIntegrity) * 100f;
        public bool CargoLost => Integrity <= 0f;

        public event Action<float, float> IntegrityChanged;
        public event Action CargoDestroyed;

        public void Configure(ArcadeForkliftController2D forklift)
        {
            vehicle = forklift;
            body = vehicle != null ? vehicle.GetComponent<Rigidbody2D>() : null;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (vehicle == null) vehicle = GetComponent<ArcadeForkliftController2D>();
            if (body == null) body = GetComponent<Rigidbody2D>();
            Integrity = maximumIntegrity;
        }

        private void OnEnable()
        {
            if (vehicle != null)
            {
                vehicle.Impacted += OnVehicleImpact;
            }
        }

        private void OnDisable()
        {
            if (vehicle != null)
            {
                vehicle.Impacted -= OnVehicleImpact;
            }
        }

        private void FixedUpdate()
        {
            if (CargoLost || body == null || vehicle == null)
            {
                return;
            }

            if (Time.time >= protectionUntil)
            {
                protectionMultiplier = 1f;
            }

            float lateralSpeed = Mathf.Abs(Vector2.Dot(body.linearVelocity, transform.right));
            float threshold = lateralStressThreshold * Mathf.Max(0.15f, stabilityMultiplier);
            if (lateralSpeed <= threshold)
            {
                return;
            }

            float excess = lateralSpeed - threshold;
            float driftFactor = vehicle.IsDrifting ? 1.25f : 1f;
            ApplyDamage(excess * lateralDamagePerSecond * driftFactor * Time.fixedDeltaTime * protectionMultiplier);
        }

        public void ApplyStabilityProtection(float damageMultiplier, float duration)
        {
            protectionMultiplier = Mathf.Clamp(damageMultiplier, 0.05f, 1f);
            protectionUntil = Mathf.Max(protectionUntil, Time.time + Mathf.Max(0.1f, duration));
        }

        public void Repair(float amount)
        {
            if (amount <= 0f || maximumIntegrity <= 0f)
            {
                return;
            }

            Integrity = Mathf.Min(maximumIntegrity, Integrity + amount);
            IntegrityChanged?.Invoke(Integrity, maximumIntegrity);
        }

        private void OnVehicleImpact(float impact)
        {
            if (impact <= 2f || CargoLost)
            {
                return;
            }

            ApplyDamage((impact - 2f) * impactDamageScale * protectionMultiplier);
        }

        private void ApplyDamage(float amount)
        {
            if (amount <= 0f || CargoLost)
            {
                return;
            }

            Integrity = Mathf.Max(0f, Integrity - amount);
            IntegrityChanged?.Invoke(Integrity, maximumIntegrity);
            if (CargoLost)
            {
                CargoDestroyed?.Invoke();
            }
        }
    }
}
