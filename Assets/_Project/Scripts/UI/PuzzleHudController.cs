using TW08.Core;
using TW08.Motion;
using TW08.Puzzle;
using TW08.Save;
using TW08.UI.Hud;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// HUD da fase de puzzle.
    ///
    /// O fluxo de progresso (registro de tentativa, gravação de conclusão e
    /// fechamento do turno no <see cref="SaveManager"/>) é o mesmo de sempre; o
    /// que mudou é que cada troca de estado agora entra animada. Toda animação é
    /// decorativa: se um tween for interrompido, o texto e o número finais já
    /// foram aplicados.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleHudController : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text movesText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text operatorText;
        [SerializeField] private Text targetText;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private Button primaryActionButton;
        [SerializeField] private string nextSceneName;
        [SerializeField] private string campaignSelectScene = "TW08_PuzzleSelect";

        [Header("Movimento")]
        [SerializeField] private Text movesValueText;
        [SerializeField] private Text rankingText;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private PuzzleShiftReportPanel reportPanel;
        [SerializeField] private ScreenFader screenFader;

        private const float AlarmBlinkSpeed = 2.6f;

        private bool bound;
        private bool attemptRegistered;
        private bool hasReport;
        private bool shiftCommitted;
        private bool exiting;
        private PuzzleShiftReport lastReport;

        private readonly AnimatedCounter moveCounter = new(HudFormat.MovesValueFormat, 0.3f);

        private MotionHandle movePunchHandle;
        private MotionHandle moveFlashHandle;
        private MotionHandle statusPunchHandle;
        private MotionHandle statusColorHandle;
        private MotionHandle shakeHandle;
        private MotionHandle rankingPunchHandle;
        private MotionHandle rankingColorHandle;

        private PuzzleHudStatus currentStatus = PuzzleHudStatus.Unavailable;
        private bool statusPrimed;
        private bool alarmActive;
        private bool lastAssisted;
        private bool rankingPrimed;

        public void Configure(
            PuzzleRuntime puzzleRuntime,
            Text levelName,
            Text moves,
            Text status,
            Button undo,
            Button redo,
            Button primaryAction)
        {
            Unbind();
            runtime = puzzleRuntime;
            levelNameText = levelName;
            movesText = moves;
            statusText = status;
            undoButton = undo;
            redoButton = redo;
            primaryActionButton = primaryAction;
            Bind();
            Refresh();
            MarkDirtyInEditor();
        }

        public void ConfigureCampaignFlow(string nextScene, string selectScene = "TW08_PuzzleSelect")
        {
            nextSceneName = nextScene;
            campaignSelectScene = string.IsNullOrWhiteSpace(selectScene) ? "TW08_PuzzleSelect" : selectScene;
            MarkDirtyInEditor();
        }

        public void ConfigureExtendedLabels(Text operatorLabel, Text targetLabel)
        {
            operatorText = operatorLabel;
            targetText = targetLabel;
            Refresh();
            MarkDirtyInEditor();
        }

        /// <summary>
        /// Liga os elementos animados. Todos são opcionais: cenas antigas, que
        /// só chamam <see cref="Configure"/>, continuam funcionando com o rótulo
        /// composto de movimentos e sem tela de conclusão.
        /// </summary>
        public void ConfigureMotion(
            Text movesValue,
            Text ranking,
            RectTransform bottomBar,
            PuzzleShiftReportPanel report,
            ScreenFader fader)
        {
            movesValueText = movesValue;
            rankingText = ranking;
            bottomPanel = bottomBar;
            reportPanel = report;
            screenFader = fader;

            moveCounter.Attach(movesValueText);
            rankingPrimed = false;
            Refresh();
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            moveCounter.Attach(movesValueText);
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
            StopMotion();
        }

        private void Bind()
        {
            if (bound || runtime == null)
            {
                return;
            }

            runtime.Initialized += OnInitialized;
            runtime.MoveApplied += OnMoveApplied;
            runtime.MoveUndone += OnBoardChanged;
            runtime.MoveRedone += OnMoveApplied;
            runtime.LevelRestarted += OnRestarted;
            runtime.LevelCompleted += OnCompleted;
            runtime.StaticDeadlockDetected += OnDeadlock;
            runtime.SwitchGroupStateChanged += OnSwitchChanged;
            runtime.AssistanceUsed += OnAssistanceUsed;

            undoButton?.onClick.AddListener(Undo);
            redoButton?.onClick.AddListener(Redo);
            primaryActionButton?.onClick.AddListener(PrimaryAction);
            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            if (runtime != null)
            {
                runtime.Initialized -= OnInitialized;
                runtime.MoveApplied -= OnMoveApplied;
                runtime.MoveUndone -= OnBoardChanged;
                runtime.MoveRedone -= OnMoveApplied;
                runtime.LevelRestarted -= OnRestarted;
                runtime.LevelCompleted -= OnCompleted;
                runtime.StaticDeadlockDetected -= OnDeadlock;
                runtime.SwitchGroupStateChanged -= OnSwitchChanged;
                runtime.AssistanceUsed -= OnAssistanceUsed;
            }

            undoButton?.onClick.RemoveListener(Undo);
            redoButton?.onClick.RemoveListener(Redo);
            primaryActionButton?.onClick.RemoveListener(PrimaryAction);
            bound = false;
        }

        private void Update()
        {
            if (!alarmActive || statusText == null)
            {
                return;
            }

            // Alarme de carga travada pisca enquanto durar. Um flash único
            // apagaria antes de o jogador desviar o olhar do tabuleiro.
            float pulse = Mathf.PingPong(Time.unscaledTime * AlarmBlinkSpeed, 1f);
            statusText.color = Color.Lerp(HudPalette.Red, HudPalette.Amber, pulse);
        }

        private void Undo() => runtime?.Undo();
        private void Redo() => runtime?.Redo();

        private void PrimaryAction()
        {
            if (runtime?.Board == null)
            {
                return;
            }

            if (runtime.Board.IsComplete)
            {
                BeginExit();
                return;
            }

            runtime.Restart();
        }

        private void OnInitialized()
        {
            // A tentativa conta uma vez por entrada na fase; um reset no meio do
            // turno não pode apagar o bônus de primeira tentativa nem inflá-lo.
            if (!attemptRegistered && runtime?.Level != null)
            {
                Object.FindFirstObjectByType<SaveManager>()?.RegisterPuzzleAttempt(runtime.Level.LevelId);
                attemptRegistered = true;
            }

            hasReport = false;
            exiting = false;
            if (reportPanel != null)
            {
                reportPanel.Hide();
            }

            Refresh();
        }

        private void OnBoardChanged(PuzzleMove _) => Refresh();
        private void OnMoveApplied(PuzzleMove move) => Refresh(move.CrateMoved);
        private void OnAssistanceUsed() => Refresh();

        private void OnRestarted()
        {
            if (reportPanel != null)
            {
                reportPanel.Hide();
            }

            Refresh();
        }

        private void OnDeadlock() => Refresh();
        private void OnSwitchChanged(string groupId, bool open)
        {
            Refresh();
            AnnounceDoor(groupId, open);
        }

        private void OnCompleted()
        {
            // Fechar o turno paga créditos, e desfazer/refazer sobre o tabuleiro
            // já completo disparava este evento de novo — cada par de teclas
            // rendia outra vez o prêmio inteiro da fase.
            if (shiftCommitted)
            {
                Refresh();
                return;
            }

            if (runtime?.Level != null && runtime.Board != null)
            {
                shiftCommitted = true;
                PuzzleProgressStore.RecordCompletion(runtime.Level, runtime.Board.MoveCount);

                SaveManager saveManager = Object.FindFirstObjectByType<SaveManager>();
                if (saveManager != null)
                {
                    lastReport = saveManager.CommitPuzzleShift(runtime.Level, runtime.BuildSummary());
                    hasReport = true;
                }
            }

            Refresh();
            ShowShiftReport();

            if (primaryActionButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(primaryActionButton.gameObject);
            }
        }

        private void Refresh() => Refresh(false);

        private void Refresh(bool cargoPushed)
        {
            if (runtime == null)
            {
                return;
            }

            if (levelNameText != null)
            {
                levelNameText.text = runtime.Level != null
                    ? HudFormat.LevelTitle(runtime.Level.SectorId, runtime.Level.DisplayName)
                    : "ROTA DESCONHECIDA";
            }

            RefreshMoves(cargoPushed);

            if (operatorText != null)
            {
                operatorText.text = HudFormat.Operator(CharacterSelectionState.SelectedCharacterId);
            }

            if (targetText != null && runtime.Level != null)
            {
                targetText.text = HudFormat.Targets(runtime.Level.PlatinumMoveLimit, runtime.Level.GoldMoveLimit);
            }

            if (undoButton != null)
            {
                undoButton.interactable = runtime.UndoCount > 0;
            }

            if (redoButton != null)
            {
                redoButton.interactable = runtime.RedoCount > 0;
            }

            RefreshStatus();
            RefreshRanking();
            RefreshPrimaryAction();
        }

        private void RefreshMoves(bool cargoPushed)
        {
            int moves = runtime.Board?.MoveCount ?? 0;

            if (movesValueText == null)
            {
                // Compatibilidade: cenas sem contador dedicado usam o rótulo composto.
                if (movesText != null)
                {
                    movesText.text = HudFormat.MoveSummary(moves, runtime.UndoCount, runtime.RedoCount);
                }

                return;
            }

            if (movesText != null)
            {
                movesText.text = HudFormat.MoveHistory(runtime.UndoCount, runtime.RedoCount);
            }

            moveCounter.Attach(movesValueText);
            if (!moveCounter.Set(moves))
            {
                return;
            }

            HudFx.Punch(
                ref movePunchHandle,
                movesValueText.transform,
                cargoPushed ? 0.2f : 0.1f,
                cargoPushed ? 0.32f : 0.22f);

            if (cargoPushed)
            {
                HudFx.Abort(ref moveFlashHandle);
                moveFlashHandle = HudFx.Flash(movesValueText, HudPalette.Cyan, HudPalette.Green, 0.3f);
            }
        }

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            PuzzleBoardModel board = runtime.Board;
            bool complete = board != null && board.IsComplete;
            bool deadlocked = board != null && !complete && SimpleDeadlockDetector.HasStaticCornerDeadlock(board);
            PuzzleHudStatus status = PuzzleHudStatusResolver.Resolve(board != null, complete, deadlocked);

            statusText.text = status == PuzzleHudStatus.Complete
                ? PuzzleHudStatusResolver.CompletionLabel(
                    PuzzleProgressStore.EvaluateMedal(runtime.Level, board.MoveCount),
                    runtime.IsAssisted,
                    lastReport.CreditsEarned,
                    hasReport)
                : PuzzleHudStatusResolver.LabelFor(status);

            if (statusPrimed && status == currentStatus)
            {
                return;
            }

            currentStatus = status;
            statusPrimed = true;
            alarmActive = PuzzleHudStatusResolver.IsAlarming(status);

            if (alarmActive)
            {
                TriggerDeadlockAlert();
                return;
            }

            HudFx.Abort(ref statusColorHandle);
            statusColorHandle = UIMotion.ColorTo(
                statusText, PuzzleHudStatusResolver.ColorFor(status), 0.28f, Ease.OutQuad);
            HudFx.Punch(ref statusPunchHandle, statusText.transform, 0.13f, 0.28f);
        }

        private void TriggerDeadlockAlert()
        {
            HudFx.Abort(ref statusColorHandle);
            if (statusText != null)
            {
                statusText.color = HudPalette.Red;
                HudFx.Punch(ref statusPunchHandle, statusText.transform, 0.2f, 0.34f);
            }

            HudFx.Shake(ref shakeHandle, bottomPanel, 13f, 0.46f);
        }

        private void AnnounceDoor(string groupId, bool open)
        {
            // O alarme de travamento tem prioridade: não sobrescreve o aviso.
            if (statusText == null || alarmActive || currentStatus != PuzzleHudStatus.Active)
            {
                return;
            }

            statusText.text = HudFormat.DoorNotice(groupId, open);
            HudFx.Abort(ref statusColorHandle);
            statusColorHandle = HudFx.Flash(
                statusText,
                open ? HudPalette.Cyan : HudPalette.Amber,
                HudPalette.Green,
                0.55f);
            HudFx.Punch(ref statusPunchHandle, statusText.transform, 0.16f, 0.3f);
        }

        private void RefreshRanking()
        {
            if (rankingText == null)
            {
                return;
            }

            bool assisted = runtime.IsAssisted;
            rankingText.text = HudFormat.RankingChip(assisted);
            Color target = assisted ? HudPalette.Amber : HudPalette.Green;

            if (!rankingPrimed)
            {
                rankingPrimed = true;
                lastAssisted = assisted;
                rankingText.color = target;
                return;
            }

            if (assisted == lastAssisted)
            {
                return;
            }

            lastAssisted = assisted;
            HudFx.Abort(ref rankingColorHandle);
            rankingColorHandle = UIMotion.ColorTo(rankingText, target, 0.32f, Ease.OutQuad);
            HudFx.Punch(ref rankingPunchHandle, rankingText.transform, 0.2f, 0.38f);
        }

        private void RefreshPrimaryAction()
        {
            if (primaryActionButton == null)
            {
                return;
            }

            Text label = primaryActionButton.GetComponentInChildren<Text>();
            if (label == null)
            {
                return;
            }

            label.text = runtime.Board != null && runtime.Board.IsComplete
                ? "PRÓXIMA [ENTER/A]"
                : "RESET [R]";
        }

        private void ShowShiftReport()
        {
            if (reportPanel == null || runtime?.Board == null)
            {
                return;
            }

            int medal = PuzzleProgressStore.EvaluateMedal(runtime.Level, runtime.Board.MoveCount);
            string title = runtime.Level != null
                ? HudFormat.LevelTitle(runtime.Level.SectorId, runtime.Level.DisplayName)
                : "TURNO ENCERRADO";

            reportPanel.Show(
                title,
                hasReport ? lastReport.Statement : null,
                hasReport ? lastReport.CreditsEarned : 0,
                hasReport ? lastReport.CreditBalance : 0,
                medal,
                runtime.IsAssisted);
        }

        private void BeginExit()
        {
            if (exiting)
            {
                return;
            }

            exiting = true;
            if (screenFader != null)
            {
                screenFader.FadeOutThen(LoadNextSceneOrMenu);
                return;
            }

            LoadNextSceneOrMenu();
        }

        private void LoadNextSceneOrMenu()
        {
            if (!string.IsNullOrWhiteSpace(nextSceneName) &&
                SceneLoader.TryLoadImmediate(nextSceneName, "próxima fase"))
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            int nextIndex = activeScene.buildIndex + 1;
            if (activeScene.buildIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex, LoadSceneMode.Single);
                return;
            }

            SceneLoader.TryLoadImmediate(campaignSelectScene, "seleção da campanha");
        }

        private void StopMotion()
        {
            alarmActive = false;
            HudFx.Finish(ref movePunchHandle);
            HudFx.Finish(ref moveFlashHandle);
            HudFx.Finish(ref statusPunchHandle);
            HudFx.Finish(ref statusColorHandle);
            HudFx.Finish(ref shakeHandle);
            HudFx.Finish(ref rankingPunchHandle);
            HudFx.Finish(ref rankingColorHandle);
            moveCounter.Stop();
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
