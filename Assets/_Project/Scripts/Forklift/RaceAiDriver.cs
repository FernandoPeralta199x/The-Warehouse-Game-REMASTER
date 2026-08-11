using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArcadeForkliftController2D))]
    [RequireComponent(typeof(RacerProgress))]
    public sealed class RaceAiDriver : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private ArcadeForkliftController2D controller;
        [SerializeField] private RacerProgress progress;
        [SerializeField, Range(0.55f, 1.15f)] private float skill = 0.88f;
        [SerializeField, Range(0f, 1f)] private float aggression = 0.55f;
        [SerializeField, Min(5f)] private float fullSteerAngle = 55f;
        [SerializeField, Min(5f)] private float brakingAngle = 80f;
        [SerializeField, Min(0f)] private float aimJitter = 0.16f;

        private Vector2 seededOffset;

        public void Configure(
            RaceManager manager,
            ArcadeForkliftController2D forklift,
            RacerProgress racerProgress,
            float driverSkill,
            float driverAggression)
        {
            raceManager = manager;
            controller = forklift;
            progress = racerProgress;
            skill = Mathf.Clamp(driverSkill, 0.55f, 1.15f);
            aggression = Mathf.Clamp01(driverAggression);
            BuildSeededOffset();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (controller == null) controller = GetComponent<ArcadeForkliftController2D>();
            if (progress == null) progress = GetComponent<RacerProgress>();
            BuildSeededOffset();
        }

        private void FixedUpdate()
        {
            if (controller == null || progress == null || raceManager == null ||
                !raceManager.RaceRunning || progress.Finished)
            {
                controller?.SetAiInput(0f, 0f, false);
                return;
            }

            if (!raceManager.TryGetCheckpointPosition(progress.NextCheckpointIndex, out Vector2 checkpoint))
            {
                controller.SetAiInput(0f, 0f, false);
                return;
            }

            Vector2 target = checkpoint + seededOffset;
            Vector2 toTarget = target - (Vector2)transform.position;
            if (toTarget.sqrMagnitude < 0.001f)
            {
                controller.SetAiInput(0f, 0.25f, false);
                return;
            }

            float angle = Vector2.SignedAngle(transform.up, toTarget.normalized);
            float steer = Mathf.Clamp(-angle / Mathf.Max(5f, fullSteerAngle), -1f, 1f);

            float absAngle = Mathf.Abs(angle);
            float cornerFactor = Mathf.InverseLerp(brakingAngle, 10f, absAngle);
            float rankAssist = Mathf.Lerp(0.92f, 1.08f, raceManager.GetNormalizedRank(progress));
            float throttle = Mathf.Clamp01(Mathf.Lerp(0.35f, 1f, cornerFactor) * skill * rankAssist);

            bool drift = absAngle > Mathf.Lerp(48f, 27f, aggression) &&
                         controller.NormalizedSpeed > 0.34f &&
                         absAngle < 125f;

            controller.SetAiInput(steer, throttle, drift);
        }

        private void BuildSeededOffset()
        {
            string id = progress != null ? progress.RacerId : gameObject.name;
            int hash = string.IsNullOrEmpty(id) ? 0 : id.GetHashCode();
            float x = (((hash & 255) / 255f) - 0.5f) * aimJitter;
            float y = ((((hash >> 8) & 255) / 255f) - 0.5f) * aimJitter;
            seededOffset = new Vector2(x, y);
        }
    }
}
