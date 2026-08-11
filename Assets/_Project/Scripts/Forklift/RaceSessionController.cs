using System;
using TW08.Core;
using TW08.Save;
using UnityEngine;

namespace TW08.Race
{
    [DisallowMultipleComponent]
    public sealed class RaceSessionController : MonoBehaviour
    {
        [SerializeField] private RaceTrackDefinition track;
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RaceCountdown countdown;
        [SerializeField] private ArcadeForkliftController2D playerVehicle;
        [SerializeField] private RacerProgress playerProgress;

        private int countdownValue = -1;

        public RaceTrackDefinition Track => track;
        public RaceManager RaceManager => raceManager;
        public RacerProgress PlayerProgress => playerProgress;
        public int CountdownValue => countdownValue;
        public bool RaceRunning => raceManager != null && raceManager.RaceRunning;
        public float ElapsedTime => raceManager != null ? raceManager.ElapsedTime : 0f;
        public float BestTime => track == null ? 0f : RaceProgressStore.GetBestTime(track.TrackId);
        public string SelectedCharacterId => CharacterSelectionState.SelectedCharacterId;

        public event Action StateChanged;
        public event Action<int> CountdownChanged;
        public event Action<float, int> PlayerFinished;

        public void Configure(
            RaceTrackDefinition trackDefinition,
            RaceManager manager,
            RaceCountdown raceCountdown,
            ArcadeForkliftController2D vehicle,
            RacerProgress progress)
        {
            track = trackDefinition;
            raceManager = manager;
            countdown = raceCountdown;
            playerVehicle = vehicle;
            playerProgress = progress;

#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            if (countdown != null)
            {
                countdown.Tick += OnCountdownTick;
                countdown.Completed += OnCountdownCompleted;
            }
            if (raceManager != null)
            {
                raceManager.RaceStarted += OnRaceStarted;
                raceManager.RacerFinished += OnRacerFinished;
            }
        }

        private void Start()
        {
            if (playerVehicle != null) playerVehicle.ControlsEnabled = false;
            float seconds = track != null && track.RaceRules != null ? track.RaceRules.CountdownSeconds : 3f;
            if (countdown != null) countdown.Begin(seconds);
            else OnCountdownCompleted();
        }

        private void OnDisable()
        {
            if (countdown != null)
            {
                countdown.Tick -= OnCountdownTick;
                countdown.Completed -= OnCountdownCompleted;
            }
            if (raceManager != null)
            {
                raceManager.RaceStarted -= OnRaceStarted;
                raceManager.RacerFinished -= OnRacerFinished;
            }
        }

        private void OnCountdownTick(int value)
        {
            countdownValue = value;
            CountdownChanged?.Invoke(value);
            StateChanged?.Invoke();
        }

        private void OnCountdownCompleted()
        {
            countdownValue = 0;
            raceManager?.StartRace();
            if (playerVehicle != null) playerVehicle.ControlsEnabled = true;
            StateChanged?.Invoke();
        }

        private void OnRaceStarted() => StateChanged?.Invoke();

        private void OnRacerFinished(RacerProgress racer)
        {
            if (racer == null || racer != playerProgress) return;
            if (playerVehicle != null) playerVehicle.ControlsEnabled = false;

            float finishTime = racer.FinishTime;
            int medal = track != null ? track.GetMedal(finishTime) : 0;
            RaceProgressStore.Record(track, finishTime);
            Object.FindFirstObjectByType<SaveManager>()?.RecordRaceCompletion(track, finishTime);
            PlayerFinished?.Invoke(finishTime, medal);
            StateChanged?.Invoke();
        }
    }
}
