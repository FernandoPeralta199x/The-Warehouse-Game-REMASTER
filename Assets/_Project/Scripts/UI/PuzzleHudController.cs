using TW08.Puzzle;
using UnityEngine;
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
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        [SerializeField] private Button restartButton;

        private bool bound;

        public void Configure(
            PuzzleRuntime puzzleRuntime,
            Text levelName,
            Text moves,
            Text status,
            Button undo,
            Button redo,
            Button restart)
        {
            Unbind();
            runtime = puzzleRuntime;
            levelNameText = levelName;
            movesText = moves;
            statusText = status;
            undoButton = undo;
            redoButton = redo;
            restartButton = restart;
            Bind();
            Refresh();
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

            runtime.MoveApplied += OnBoardChanged;
            runtime.MoveUndone += OnBoardChanged;
            runtime.MoveRedone += OnBoardChanged;
            runtime.LevelRestarted += OnRestarted;
            runtime.LevelCompleted += OnCompleted;
            runtime.StaticDeadlockDetected += OnDeadlock;

            undoButton?.onClick.AddListener(Undo);
            redoButton?.onClick.AddListener(Redo);
            restartButton?.onClick.AddListener(Restart);
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
                runtime.MoveApplied -= OnBoardChanged;
                runtime.MoveUndone -= OnBoardChanged;
                runtime.MoveRedone -= OnBoardChanged;
                runtime.LevelRestarted -= OnRestarted;
                runtime.LevelCompleted -= OnCompleted;
                runtime.StaticDeadlockDetected -= OnDeadlock;
            }

            undoButton?.onClick.RemoveListener(Undo);
            redoButton?.onClick.RemoveListener(Redo);
            restartButton?.onClick.RemoveListener(Restart);
            bound = false;
        }

        private void Undo() => runtime?.Undo();
        private void Redo() => runtime?.Redo();
        private void Restart() => runtime?.Restart();

        private void OnBoardChanged(PuzzleMove _) => Refresh();
        private void OnRestarted() => Refresh();
        private void OnCompleted() => Refresh();
        private void OnDeadlock() => Refresh();

        private void Refresh()
        {
            if (runtime == null)
            {
                return;
            }

            if (levelNameText != null)
            {
                levelNameText.text = runtime.Level != null
                    ? runtime.Level.DisplayName.ToUpperInvariant()
                    : "ROTA DESCONHECIDA";
            }

            if (movesText != null)
            {
                int moves = runtime.Board?.MoveCount ?? 0;
                movesText.text = $"MOVIMENTOS {moves:000}   UNDO {runtime.UndoCount:00}   REDO {runtime.RedoCount:00}";
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
                statusText.text = "ROTA LIBERADA // TURNO CONCLUÍDO";
                return;
            }

            if (SimpleDeadlockDetector.HasStaticCornerDeadlock(board))
            {
                statusText.text = "ALERTA: CARGA TRAVADA // USE UNDO";
                return;
            }

            statusText.text = "ROTA ATIVA";
        }
    }
}
