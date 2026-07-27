using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RacerProgress : MonoBehaviour
    {
        [SerializeField] private string racerId = "player";
        [SerializeField] private RaceManager raceManager;

        public string RacerId => racerId;
        public int CurrentLap { get; private set; } = 1;
        public int NextCheckpointIndex { get; private set; } = 1;
        public bool Finished { get; private set; }
        public float FinishTime { get; private set; }

        private void OnEnable()
        {
            raceManager?.Register(this);
        }

        private void OnDisable()
        {
            raceManager?.Unregister(this);
        }

        public void Configure(RaceManager manager, string id)
        {
            raceManager = manager;
            racerId = id;
        }

        public void ResetProgress()
        {
            CurrentLap = 1;
            NextCheckpointIndex = 1;
            Finished = false;
            FinishTime = 0f;
        }

        internal void AdvanceCheckpoint(int checkpointCount, int totalLaps, float elapsedTime)
        {
            if (Finished)
            {
                return;
            }

            if (NextCheckpointIndex == 0)
            {
                CurrentLap++;
                if (CurrentLap > totalLaps)
                {
                    Finished = true;
                    FinishTime = elapsedTime;
                    return;
                }

                NextCheckpointIndex = checkpointCount > 1 ? 1 : 0;
                return;
            }

            NextCheckpointIndex++;
            if (NextCheckpointIndex >= checkpointCount)
            {
                NextCheckpointIndex = 0;
            }
        }
    }
}
