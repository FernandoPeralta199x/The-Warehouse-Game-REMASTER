using TW08.Core;
using TW08.Puzzle;
using TW08.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
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

        private bool bound;

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

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (bound || runtime == null)
            {
                return;
            }

            runtime.Initialized += OnInitialized;
            runtime.MoveApplied += OnBoardChanged;
            runtime.MoveUndone += OnBoardChanged;
            runtime.MoveRedone += OnBoardChanged;
            runtime.LevelRestarted += OnRestarted;
            runtime.LevelCompleted += OnCompleted;
            runtime.StaticDeadlockDetected += OnDeadlock;
            runtime.SwitchGroupStateChanged += OnSwitchChanged;

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
                runtime.MoveApplied -= OnBoardChanged;
                runtime.MoveUndone -= OnBoardChanged;
                runtime.MoveRedone -= OnBoardChanged;
                runtime.LevelRestarted -= OnRestarted;
                runtime.LevelCompleted -= OnCompleted;
                runtime.StaticDeadlockDetected -= OnDeadlock;
                runtime.SwitchGroupStateChanged -= OnSwitchChanged;
            }

            undoButton?.onClick.RemoveListener(Undo);
            redoButton?.onClick.RemoveListener(Redo);
            primaryActionButton?.onClick.RemoveListener(PrimaryAction);
            bound = false;
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
                LoadNextSceneOrMenu();
                return;
            }

            runtime.Restart();
        }

        private void OnInitialized() => Refresh();
        private void OnBoardChanged(PuzzleMove _) => Refresh();
        private void OnRestarted() => Refresh();
        private void OnDeadlock() => Refresh();
        private void OnSwitchChanged(string _, bool __) => Refresh();

        private void OnCompleted()
        {
            if (runtime?.Level != null && runtime.Board != null)
            {
                PuzzleProgressStore.RecordCompletion(runtime.Level, runtime.Board.MoveCount);
                SaveManager saveManager = Object.FindFirstObjectByType<SaveManager>();
                saveManager?.RecordPuzzleCompletion(runtime.Level, runtime.Board.MoveCount);
            }

            Refresh();
            if (primaryActionButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(primaryActionButton.gameObject);
            }
        }

        private void Refresh()
        {
            if (runtime == null)
            {
                return;
            }

            if (levelNameText != null)
            {
                levelNameText.text = runtime.Level != null
                    ? $"{runtime.Level.SectorId} // {runtime.Level.DisplayName}".ToUpperInvariant()
                    : "ROTA DESCONHECIDA";
            }

            if (movesText != null)
            {
                int moves = runtime.Board?.MoveCount ?? 0;
                movesText.text = $"MOVIMENTOS {moves:000}   UNDO {runtime.UndoCount:00}   REDO {runtime.RedoCount:00}";
            }

            if (operatorText != null)
            {
                operatorText.text = "OPERADOR // " + CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }

            if (targetText != null && runtime.Level != null)
            {
                targetText.text = $"PLAT {runtime.Level.PlatinumMoveLimit:000} // GOLD {runtime.Level.GoldMoveLimit:000}";
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
            RefreshPrimaryAction();
        }

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            PuzzleBoardModel board = runtime.Board;
            if (board == null)
            {
                statusText.text = "ROTA INDISPONÍVEL";
                return;
            }

            if (board.IsComplete)
            {
                int medal = PuzzleProgressStore.EvaluateMedal(runtime.Level, board.MoveCount);
                statusText.text = $"ROTA LIBERADA // MEDALHA {medal}";
                return;
            }

            if (SimpleDeadlockDetector.HasStaticCornerDeadlock(board))
            {
                statusText.text = "ALERTA: CARGA TRAVADA // USE UNDO";
                return;
            }

            statusText.text = "ROTA ATIVA";
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
