using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceManager : MonoBehaviour
    {
        [SerializeField] private List<RaceCheckpoint> checkpoints = new();
        [SerializeField, Min(1)] private int totalLaps = 3;
        [SerializeField] private bool autoStart = true;

        private readonly List<RacerProgress> racers = new();
        private float elapsedTime;

        public bool RaceRunning { get; private set; }
        public float ElapsedTime => elapsedTime;
        public int TotalLaps => totalLaps;
        public int CheckpointCount => checkpoints.Count;
        public IReadOnlyList<RacerProgress> Racers => racers;

        public event Action RaceStarted;
        public event Action<RacerProgress> RacerFinished;

        private void Start()
        {
            checkpoints = checkpoints.Where(c => c != null).OrderBy(c => c.CheckpointIndex).ToList();
            if (autoStart)
            {
                StartRace();
            }
        }

        private void Update()
        {
            if (RaceRunning)
            {
                elapsedTime += Time.deltaTime;
            }
        }

        public void Configure(IEnumerable<RaceCheckpoint> orderedCheckpoints, int laps)
        {
            checkpoints = orderedCheckpoints?.OrderBy(c => c.CheckpointIndex).ToList() ?? new List<RaceCheckpoint>();
            totalLaps = Mathf.Max(1, laps);
        }

        public void Register(RacerProgress racer)
        {
            if (racer != null && !racers.Contains(racer))
            {
                racers.Add(racer);
            }
        }

        public void Unregister(RacerProgress racer)
        {
            racers.Remove(racer);
        }

        public void StartRace()
        {
            elapsedTime = 0f;
            RaceRunning = true;
            foreach (RacerProgress racer in racers)
            {
                racer.ResetProgress();
            }
            RaceStarted?.Invoke();
        }

        public void NotifyCheckpoint(RacerProgress racer, int checkpointIndex)
        {
            if (!RaceRunning || racer == null || racer.Finished || checkpoints.Count == 0)
            {
                return;
            }

            if (checkpointIndex != racer.NextCheckpointIndex)
            {
                return;
            }

            racer.AdvanceCheckpoint(checkpoints.Count, totalLaps, elapsedTime);
            if (racer.Finished)
            {
                RacerFinished?.Invoke(racer);
                if (racers.Count > 0 && racers.All(r => r.Finished))
                {
                    RaceRunning = false;
                }
            }
        }

        public float GetNormalizedRank(RacerProgress target)
        {
            if (target == null || racers.Count <= 1)
            {
                return 0.5f;
            }

            List<RacerProgress> ordered = racers
                .OrderByDescending(GetProgressScore)
                .ThenBy(r => r.Finished ? r.FinishTime : float.MaxValue)
                .ToList();
            int index = ordered.IndexOf(target);
            return index < 0 ? 0.5f : index / (float)(ordered.Count - 1);
        }

        private float GetProgressScore(RacerProgress racer)
        {
            int lastPassed = racer.NextCheckpointIndex == 0
                ? Mathf.Max(0, checkpoints.Count - 1)
                : racer.NextCheckpointIndex - 1;
            float baseScore = (racer.CurrentLap - 1) * checkpoints.Count + lastPassed;

            if (checkpoints.Count == 0 || racer.NextCheckpointIndex < 0 || racer.NextCheckpointIndex >= checkpoints.Count)
            {
                return baseScore;
            }

            float distanceToNext = Vector2.Distance(
                racer.transform.position,
                checkpoints[racer.NextCheckpointIndex].transform.position);
            return baseScore - distanceToNext * 0.001f;
        }
    }
}
