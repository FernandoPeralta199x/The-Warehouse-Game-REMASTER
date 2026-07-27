using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class WaypointRaceAI : MonoBehaviour
    {
        [SerializeField] private ArcadeForkliftController2D controller;
        [SerializeField] private RaceWaypointPath path;
        [SerializeField, Min(0.1f)] private float waypointRadius = 1.2f;
        [SerializeField, Range(5f, 90f)] private float driftAngle = 42f;
        [SerializeField, Range(0.1f, 1f)] private float cautiousThrottle = 0.55f;
        private int waypointIndex;

        public void Configure(ArcadeForkliftController2D forklift, RaceWaypointPath waypointPath)
        {
            controller = forklift;
            path = waypointPath;
        }

        private void Update()
        {
            if (controller == null || path == null || path.Count == 0)
            {
                return;
            }

            Transform target = path.GetWaypoint(waypointIndex);
            if (target == null)
            {
                return;
            }

            Vector2 toTarget = target.position - transform.position;
            if (toTarget.magnitude <= waypointRadius)
            {
                waypointIndex = (waypointIndex + 1) % path.Count;
                target = path.GetWaypoint(waypointIndex);
                toTarget = target.position - transform.position;
            }

            float signedAngle = Vector2.SignedAngle(transform.up, toTarget.normalized);
            float steer = Mathf.Clamp(signedAngle / 45f, -1f, 1f);
            float absoluteAngle = Mathf.Abs(signedAngle);
            float throttle = absoluteAngle > 70f ? cautiousThrottle : 1f;
            bool drift = absoluteAngle >= driftAngle && controller.NormalizedSpeed > 0.35f;
            controller.SetAiInput(steer, throttle, drift);
        }
    }
}
