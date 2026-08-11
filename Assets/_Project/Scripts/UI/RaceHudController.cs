using TW08.Core;
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

        private void OnEnable()
        {
            if (session != null)
            {
                session.StateChanged += Refresh;
                session.CountdownChanged += OnCountdownChanged;
                session.PlayerFinished += OnPlayerFinished;
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
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.StateChanged -= Refresh;
                session.CountdownChanged -= OnCountdownChanged;
                session.PlayerFinished -= OnPlayerFinished;
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
