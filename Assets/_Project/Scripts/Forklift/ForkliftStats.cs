using UnityEngine;

namespace TW08.Race
{
    [CreateAssetMenu(fileName = "ForkliftStats", menuName = "TW08/Race/Forklift Stats")]
    public sealed class ForkliftStats : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField, Min(0.1f)] private float maxForwardSpeed = 12f;
        [SerializeField, Min(0.1f)] private float maxReverseSpeed = 5f;
        [SerializeField, Min(0.1f)] private float acceleration = 16f;
        [SerializeField, Min(0.1f)] private float brakeForce = 22f;
        [SerializeField, Range(0f, 1f)] private float rollingResistance = 0.05f;

        [Header("Steering")]
        [SerializeField, Min(1f)] private float steeringDegreesPerSecond = 150f;
        [SerializeField, Range(0f, 1f)] private float lowSpeedSteering = 0.35f;
        [SerializeField, Range(0f, 1f)] private float normalLateralRetention = 0.18f;
        [SerializeField, Range(0f, 1f)] private float driftLateralRetention = 0.68f;

        [Header("Drift Boost")]
        [SerializeField, Min(0f)] private float minimumDriftCharge = 0.45f;
        [SerializeField, Min(0.1f)] private float maximumDriftCharge = 2.2f;
        [SerializeField, Min(1f)] private float driftBoostMultiplier = 1.35f;
        [SerializeField, Min(0.1f)] private float driftBoostDuration = 0.8f;

        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxReverseSpeed => maxReverseSpeed;
        public float Acceleration => acceleration;
        public float BrakeForce => brakeForce;
        public float RollingResistance => rollingResistance;
        public float SteeringDegreesPerSecond => steeringDegreesPerSecond;
        public float LowSpeedSteering => lowSpeedSteering;
        public float NormalLateralRetention => normalLateralRetention;
        public float DriftLateralRetention => driftLateralRetention;
        public float MinimumDriftCharge => minimumDriftCharge;
        public float MaximumDriftCharge => maximumDriftCharge;
        public float DriftBoostMultiplier => driftBoostMultiplier;
        public float DriftBoostDuration => driftBoostDuration;
    }
}
