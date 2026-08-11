using TW08.Core;
using TW08.Input;
using UnityEngine;

namespace TW08.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class PuzzlePlayerController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private PuzzleRuntime runtime;

        private void OnEnable()
        {
            if (input == null || runtime == null)
            {
                return;
            }

            input.SetMode(GameMode.Puzzle);
            input.PuzzleMoveRequested += OnMove;
            input.PuzzleUndoRequested += OnUndo;
            input.PuzzleRedoRequested += OnRedo;
            input.PuzzleRestartRequested += OnRestart;
        }

        private void OnDisable()
        {
            if (input == null)
            {
                return;
            }

            input.PuzzleMoveRequested -= OnMove;
            input.PuzzleUndoRequested -= OnUndo;
            input.PuzzleRedoRequested -= OnRedo;
            input.PuzzleRestartRequested -= OnRestart;
        }

        public void Configure(GameInput gameInput, PuzzleRuntime puzzleRuntime)
        {
            input = gameInput;
            runtime = puzzleRuntime;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnMove(GridCoordinate direction) => runtime.TryMove(direction);
        private void OnUndo() => runtime.Undo();
        private void OnRedo() => runtime.Redo();
        private void OnRestart() => runtime.Restart();
    }
}
