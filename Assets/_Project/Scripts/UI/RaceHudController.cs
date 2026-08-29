using System.Collections;
using TW08.Core;
using TW08.Motion;
using TW08.PowerUps;
using TW08.Race;
using TW08.UI.Hud;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// HUD da corrida.
    ///
    /// Volta e checkpoint são lidos por polling do <see cref="RacerProgress"/>:
    /// o progresso do piloto não publica eventos, e a HUD não pode alterar a
    /// camada de corrida só para se avisar de uma passagem de checkpoint.
    /// </summary>
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

        [Header("Movimento")]
        [SerializeField] private ArcadeForkliftController2D playerVehicle;
        [SerializeField] private Text speedText;
        [SerializeField] private Text lastLapText;
        [SerializeField] private Text checkpointText;
        [SerializeField] private RaceResultPanel resultPanel;
        [SerializeField] private ScreenFader screenFader;

        private const float SpeedResponse = 9f;
        private const float LastLapBlinkSpeed = 2.2f;
        private const float StatusHoldSeconds = 1.15f;

        private MotionHandle statusPopHandle;
        private MotionHandle statusFadeHandle;
        private MotionHandle lapPunchHandle;
        private MotionHandle lapFlashHandle;
        private MotionHandle timerPunchHandle;
        private MotionHandle positionPunchHandle;
        private MotionHandle checkpointPopHandle;
        private MotionHandle checkpointFadeHandle;
        private MotionHandle lastLapPopHandle;
        private MotionHandle itemPunchHandle;

        private Coroutine statusRoutine;
        private float displayedSpeed;
        private int lastKnownLap = -1;
        private int lastCheckpointIndex = -1;
        private bool lastLapAnnounced;
        private bool finished;
        private bool leaving;

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

        /// <summary>
        /// Liga os elementos animados. Todos opcionais: uma cena que só chame
        /// <see cref="Configure"/> continua com a HUD estática de antes.
        /// </summary>
        public void ConfigureMotion(
            ArcadeForkliftController2D vehicle,
            Text speed,
            Text lastLap,
            Text checkpoint,
            RaceResultPanel result,
            ScreenFader fader)
        {
            playerVehicle = vehicle;
            speedText = speed;
            lastLapText = lastLap;
            checkpointText = checkpoint;
            resultPanel = result;
            screenFader = fader;
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
            PrimeProgressTrackers();
            HideTransientLabels();
            Refresh();
            RefreshItem(playerInventory != null ? playerInventory.Stored : null);
        }

        private void Update()
        {
            if (session != null && session.RaceRunning && timerText != null)
            {
                timerText.text = HudFormat.Time(session.ElapsedTime);
            }

            RefreshLap();
            RefreshPosition();
            RefreshSpeed();
            RefreshCargo();
            BlinkLastLap();
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

            StopMotion();
        }

        public void RestartRace()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            LeaveTo(activeSceneName, "reinício da corrida");
        }

        public void ExitRace()
        {
            LeaveTo(exitSceneName, "seleção de pistas");
        }

        private void LeaveTo(string sceneName, string context)
        {
            if (leaving)
            {
                return;
            }

            leaving = true;
            if (screenFader != null)
            {
                screenFader.FadeOutThen(() => SceneLoader.TryLoadImmediate(sceneName, context));
                return;
            }

            SceneLoader.TryLoadImmediate(sceneName, context);
        }

        private void Refresh()
        {
            if (session == null)
            {
                return;
            }

            if (trackText != null)
            {
                trackText.text = session.Track != null
                    ? session.Track.DisplayName.ToUpperInvariant()
                    : "N-8 LOGISTICS RUSH";
            }

            if (pilotText != null)
            {
                pilotText.text = "OPERADOR // " + session.SelectedCharacterId.ToUpperInvariant();
            }

            if (bestText != null)
            {
                bestText.text = HudFormat.BestTime(session.BestTime);
            }

            if (timerText != null)
            {
                timerText.text = HudFormat.Time(session.ElapsedTime);
            }

            if (statusText != null && !finished)
            {
                if (session.CountdownValue > 0)
                {
                    statusText.text = session.CountdownValue.ToString();
                }
                else if (session.RaceRunning && statusRoutine == null)
                {
                    statusText.text = "ROTA ATIVA";
                }
            }

            RefreshCargo();
        }

        private void RefreshLap()
        {
            if (session == null || session.PlayerProgress == null)
            {
                return;
            }

            int total = session.Track != null ? session.Track.Laps : 1;
            int lap = Mathf.Min(session.PlayerProgress.CurrentLap, total);

            if (lapText != null)
            {
                lapText.text = HudFormat.Lap(lap, total);
            }

            if (lap == lastKnownLap)
            {
                return;
            }

            bool firstRead = lastKnownLap < 0;
            lastKnownLap = lap;
            if (firstRead)
            {
                return;
            }

            HudFx.Punch(ref lapPunchHandle, lapText != null ? lapText.transform : null, 0.22f, 0.34f);
            HudFx.Punch(ref timerPunchHandle, timerText != null ? timerText.transform : null, 0.12f, 0.28f);
            if (lapText != null)
            {
                HudFx.Abort(ref lapFlashHandle);
                lapFlashHandle = HudFx.Flash(lapText, Color.white, HudPalette.Amber, 0.42f);
            }

            if (total > 1 && lap >= total)
            {
                AnnounceLastLap();
            }
        }

        private void AnnounceLastLap()
        {
            if (lastLapAnnounced || lastLapText == null)
            {
                return;
            }

            lastLapAnnounced = true;
            lastLapText.gameObject.SetActive(true);
            lastLapText.text = "ÚLTIMA VOLTA";
            lastLapText.color = HudPalette.WithAlpha(HudPalette.Amber, 0f);
            UIMotion.FadeTo(lastLapText, 1f, 0.26f, Ease.OutQuad);
            HudFx.PopIn(ref lastLapPopHandle, lastLapText.transform, 0.42f, 0.6f);
        }

        private void BlinkLastLap()
        {
            if (!lastLapAnnounced || finished || lastLapText == null)
            {
                return;
            }

            float pulse = Mathf.PingPong(Time.unscaledTime * LastLapBlinkSpeed, 1f);
            lastLapText.color = Color.Lerp(HudPalette.Amber, HudPalette.Red, pulse);
        }

        private void RefreshPosition()
        {
            if (session == null || session.RaceManager == null || session.PlayerProgress == null)
            {
                return;
            }

            int position = session.RaceManager.GetRacePosition(session.PlayerProgress);
            int count = Mathf.Max(1, session.RaceManager.RacerCount);

            if (positionText != null)
            {
                positionText.text = HudFormat.Position(position, count);
            }

            int checkpoint = session.PlayerProgress.NextCheckpointIndex;
            if (checkpoint == lastCheckpointIndex)
            {
                return;
            }

            bool firstRead = lastCheckpointIndex < 0;
            lastCheckpointIndex = checkpoint;
            if (firstRead)
            {
                return;
            }

            AnnounceCheckpoint();
        }

        private void AnnounceCheckpoint()
        {
            HudFx.Punch(ref positionPunchHandle, positionText != null ? positionText.transform : null, 0.16f, 0.3f);

            if (checkpointText == null)
            {
                return;
            }

            checkpointText.gameObject.SetActive(true);
            checkpointText.text = "CHECKPOINT";
            checkpointText.color = HudPalette.WithAlpha(HudPalette.Green, 1f);
            HudFx.PopIn(ref checkpointPopHandle, checkpointText.transform, 0.26f, 0.7f);
            HudFx.Abort(ref checkpointFadeHandle);
            checkpointFadeHandle = UIMotion.FadeTo(checkpointText, 0f, 0.6f, Ease.InQuad, 0.24f);
        }

        private void RefreshSpeed()
        {
            if (speedText == null || playerVehicle == null)
            {
                return;
            }

            // Suavização exponencial: o velocímetro precisa acompanhar a inércia
            // da empilhadeira em vez de pular a cada frame de física.
            float blend = 1f - Mathf.Exp(-SpeedResponse * Time.unscaledDeltaTime);
            displayedSpeed = Mathf.Lerp(displayedSpeed, playerVehicle.CurrentSpeed, blend);
            speedText.text = HudFormat.Speed(displayedSpeed);
            speedText.color = Color.Lerp(
                HudPalette.Cyan, HudPalette.Amber, Mathf.Clamp01(playerVehicle.NormalizedSpeed));
        }

        private void OnCountdownChanged(int value)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = value > 0 ? value.ToString() : "GO";
            Color accent = value > 0 ? HudPalette.Amber : HudPalette.Green;
            statusText.color = HudPalette.WithAlpha(accent, 0.2f);

            HudFx.Abort(ref statusFadeHandle);
            statusFadeHandle = UIMotion.FadeTo(statusText, 1f, 0.14f, Ease.OutQuad);
            HudFx.PopIn(ref statusPopHandle, statusText.transform, value > 0 ? 0.32f : 0.46f, value > 0 ? 0.55f : 0.35f);

            if (value <= 0)
            {
                RestartStatusHold();
            }
        }

        /// <summary>Deixa o "GO" respirar antes de a faixa voltar ao texto de rota.</summary>
        private void RestartStatusHold()
        {
            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
            }

            if (!isActiveAndEnabled)
            {
                return;
            }

            statusRoutine = StartCoroutine(ClearStatusAfterHold());
        }

        private IEnumerator ClearStatusAfterHold()
        {
            yield return new WaitForSecondsRealtime(StatusHoldSeconds);

            if (statusText != null && !finished)
            {
                statusText.text = "ROTA ATIVA";
                statusText.color = HudPalette.WithAlpha(HudPalette.Green, 0.45f);
            }

            statusRoutine = null;
        }

        private void OnPlayerFinished(float time, int medal)
        {
            finished = true;

            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
                statusRoutine = null;
            }

            if (statusText != null)
            {
                statusText.text = $"FINALIZADO // MEDALHA {medal}";
                statusText.color = HudPalette.WithAlpha(HudPalette.Cyan, 0.25f);
                HudFx.Abort(ref statusFadeHandle);
                statusFadeHandle = UIMotion.FadeTo(statusText, 1f, 0.2f, Ease.OutQuad);
                HudFx.PopIn(ref statusPopHandle, statusText.transform, 0.4f, 0.6f);
            }

            if (timerText != null)
            {
                timerText.text = HudFormat.Time(time);
                HudFx.Punch(ref timerPunchHandle, timerText.transform, 0.2f, 0.36f);
            }

            if (bestText != null && session != null)
            {
                bestText.text = HudFormat.BestTime(session.BestTime);
            }

            HideLastLap();
            RefreshCargo();
            ShowResultPanel(time, medal);
        }

        private void ShowResultPanel(float time, int medal)
        {
            if (resultPanel == null)
            {
                return;
            }

            string trackName = session != null && session.Track != null
                ? session.Track.DisplayName
                : "N-8 LOGISTICS RUSH";
            float best = session != null ? session.BestTime : 0f;

            resultPanel.Show(trackName, time, best, medal, BuildCargoLine());
        }

        private void RefreshItem(PowerUpDefinition definition)
        {
            if (itemText == null)
            {
                return;
            }

            itemText.text = HudFormat.Item(definition != null ? definition.DisplayName : null);
            if (definition != null)
            {
                HudFx.Punch(ref itemPunchHandle, itemText.transform, 0.18f, 0.32f);
            }
        }

        private void RefreshCargo()
        {
            if (cargoText != null)
            {
                cargoText.text = BuildCargoLine();
            }
        }

        private string BuildCargoLine()
        {
            RaceCargoController cargo = session != null ? session.PlayerCargo : null;
            if (cargo == null)
            {
                return "CARGA // --";
            }

            float normalized = cargo.MaximumIntegrity <= 0f
                ? 0f
                : cargo.Integrity / cargo.MaximumIntegrity;
            return HudFormat.CargoIntegrity(normalized, cargo.CargoLost);
        }

        private void PrimeProgressTrackers()
        {
            if (session == null || session.PlayerProgress == null)
            {
                return;
            }

            int total = session.Track != null ? session.Track.Laps : 1;
            lastKnownLap = Mathf.Min(session.PlayerProgress.CurrentLap, total);
            lastCheckpointIndex = session.PlayerProgress.NextCheckpointIndex;
        }

        private void HideTransientLabels()
        {
            HideLastLap();

            if (checkpointText != null)
            {
                checkpointText.color = HudPalette.WithAlpha(HudPalette.Green, 0f);
            }
        }

        private void HideLastLap()
        {
            if (lastLapText == null)
            {
                return;
            }

            HudFx.Finish(ref lastLapPopHandle);
            lastLapText.color = HudPalette.WithAlpha(HudPalette.Amber, 0f);
        }

        private void StopMotion()
        {
            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
                statusRoutine = null;
            }

            HudFx.Finish(ref statusPopHandle);
            HudFx.Finish(ref statusFadeHandle);
            HudFx.Finish(ref lapPunchHandle);
            HudFx.Finish(ref lapFlashHandle);
            HudFx.Finish(ref timerPunchHandle);
            HudFx.Finish(ref positionPunchHandle);
            HudFx.Finish(ref checkpointPopHandle);
            HudFx.Finish(ref checkpointFadeHandle);
            HudFx.Finish(ref lastLapPopHandle);
            HudFx.Finish(ref itemPunchHandle);
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
