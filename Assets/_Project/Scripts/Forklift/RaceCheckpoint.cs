using UnityEngine;

namespace TW08.Race
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class RaceCheckpoint : MonoBehaviour
    {
        [SerializeField, Min(0)] private int checkpointIndex;
        [SerializeField] private RaceManager raceManager;

        public int CheckpointIndex => checkpointIndex;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        public void Configure(RaceManager manager, int index)
        {
            raceManager = manager;
            checkpointIndex = Mathf.Max(0, index);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RacerProgress racer = other.GetComponentInParent<RacerProgress>();
            if (racer != null)
            {
                raceManager?.NotifyCheckpoint(racer, checkpointIndex);
            }
        }
    }
}
