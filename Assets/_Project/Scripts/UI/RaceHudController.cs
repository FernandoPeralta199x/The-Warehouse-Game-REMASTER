using TW08.Core;
using TW08.PowerUps;
using TW08.Race;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RaceHudController : MonoBehaviour
    {
        [SerializeField] private RaceSessionController session;
        [SerializeField] private Text trackText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text lapText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text pilotText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text positionText;
        [SerializeField] private Text itemText;
        [SerializeField] private Text cargoText;
        [SerializeField] private PowerUpInventory playerInventory;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private string exitSceneName = "TW08_RaceSelect";

        public void Configure(
            RaceSessionController raceSession,
            Text trackLabel,
            Text timerLabel,
            Text lapLabel,
            Text bestLabel,
            Text pilotLabel,
            Text statusLabel,
            Button restart,
            Button exit,
            string backScene)
        {
            session = raceSession;
            trackText = trackLabel;
            timerText = timerLabel;
            lapText = lapLabel;
            bestText = bestLabel;
            pilotText = pilotLabel;
            statusText = statusLabel;
            restartButton = restart;
            exitButton = exit;
            exitSceneName = backScene;
            MarkDirtyInEditor();
        }

        public void ConfigureArcadeOverlay(
            Text positionLabel,
            Text itemLabel,
            Text cargoLabel,
            PowerUpInventory inventory)
        {
            positionText = positionLabel;
            itemText = itemLabel;
            cargoText = cargoLabel;
            playerInventory = inventory;
            RefreshItem(playerInventory != null ? playerInventory.Stored : null);
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            if (session != null)
            {
                session.StateChanged += Refresh;
                session.CountdownChanged += OnCountdownChanged;
                session.PlayerFinished += OnPlayerFinished;
            }

            if (playerInventory != null)
            {
                playerInventory.StoredChanged += RefreshItem;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartRace);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ExitRace);
            }
        }

        private void Start()
        {
            Refresh();
            RefreshItem(playerInventory != null ? playerInventory.Stored : null);
        }

        private void Update()
        {
            if (session != null && session.RaceRunning && timerText != null)
            {
                timerText.text = FormatTime(session.ElapsedTime);
            }

            if (session != null && session.PlayerProgress != null && lapText != null)
            {
                int total = session.Track != null ? session.Track.Laps : 1;
                lapText.text = $"VOLTA {Mathf.Min(session.PlayerProgress.CurrentLap, total):00}/{total:00}";
            }

            if (positionText != null && session != null && session.RaceManager != null && session.PlayerProgress != null)
            {
                int position = session.RaceManager.GetRacePosition(session.PlayerProgress);
                int count = Mathf.Max(1, session.RaceManager.RacerCount);
                positionText.text = position > 0 ? $"POS {position:00}/{count:00}" : $"POS --/{count:00}";
            }

            RefreshCargo();
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.StateChanged -= Refresh;
                session.CountdownChanged -= OnCountdownChanged;
                session.PlayerFinished -= OnPlayerFinished;
            }

            if (playerInventory != null)
            {
                playerInventory.StoredChanged -= RefreshItem;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartRace);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(ExitRace);
            }
        }

        public void RestartRace()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            SceneLoader.TryLoadImmediate(activeSceneName, "reinício da corrida");
        }

        public void ExitRace()
        {
            SceneLoader.TryLoadImmediate(exitSceneName, "seleção de pistas");
        }

        private void Refresh()
        {
            if (session == null)
            {
                return;
            }

            if (trackText != null)
            {
                trackText.text = session.Track != null ? session.Track.DisplayName.ToUpperInvariant() : "N-8 LOGISTICS RUSH";
            }

            if (pilotText != null)
            {
                pilotText.text = "OPERADOR // " + session.SelectedCharacterId.ToUpperInvariant();
            }

            if (bestText != null)
            {
                float best = session.BestTime;
                bestText.text = best > 0f ? "BEST " + FormatTime(best) : "BEST --:--.---";
            }

            if (timerText != null)
            {
                timerText.text = FormatTime(session.ElapsedTime);
            }

            if (statusText != null && session.CountdownValue > 0)
            {
                statusText.text = session.CountdownValue.ToString();
            }
            else if (statusText != null && session.RaceRunning)
            {
                statusText.text = "ROTA ATIVA";
            }

            RefreshCargo();
        }

        private void OnCountdownChanged(int value)
        {
            if (statusText != null)
            {
                statusText.text = value > 0 ? value.ToString() : "GO";
            }
        }

        private void OnPlayerFinished(float time, int medal)
        {
            if (statusText != null)
            {
                statusText.text = $"FINALIZADO // MEDALHA {medal}";
            }

            if (timerText != null)
            {
                timerText.text = FormatTime(time);
            }

            if (bestText != null)
            {
                bestText.text = "BEST " + FormatTime(session.BestTime);
            }

            RefreshCargo();
        }

        private void RefreshItem(PowerUpDefinition definition)
        {
            if (itemText == null)
            {
                return;
            }

            itemText.text = definition != null
                ? "ITEM // " + definition.DisplayName.ToUpperInvariant()
                : "ITEM // --";
        }

        private void RefreshCargo()
        {
            if (cargoText == null || session == null)
            {
                return;
            }

            RaceCargoController cargo = session.PlayerCargo;
            if (cargo == null)
            {
                cargoText.text = "CARGA // --";
                return;
            }

            float integrity = cargo.MaximumIntegrity <= 0f
                ? 0f
                : Mathf.Clamp01(cargo.Integrity / cargo.MaximumIntegrity) * 100f;
            cargoText.text = cargo.CargoLost
                ? "CARGA // PERDIDA"
                : $"CARGA // {integrity:000}%";
        }

        private static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remainder = seconds - minutes * 60f;
            return $"{minutes:00}:{remainder:00.000}";
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
