using System.Collections.Generic;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceWaypointPath : MonoBehaviour
    {
        [SerializeField] private List<Transform> waypoints = new();

        public int Count => waypoints.Count;

        public Transform GetWaypoint(int index)
        {
            if (waypoints.Count == 0)
            {
                return null;
            }

            int wrapped = ((index % waypoints.Count) + waypoints.Count) % waypoints.Count;
            return waypoints[wrapped];
        }

        public void Configure(IEnumerable<Transform> points)
        {
            waypoints = points == null ? new List<Transform>() : new List<Transform>(points);
        }
    }
}
