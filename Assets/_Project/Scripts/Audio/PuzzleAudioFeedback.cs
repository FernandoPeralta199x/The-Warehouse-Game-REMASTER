using TW08.Puzzle;
using UnityEngine;

namespace TW08.Audio
{
    [DisallowMultipleComponent]
    public sealed class PuzzleAudioFeedback : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private TW08AudioCatalog catalog;

        public void Configure(PuzzleRuntime puzzleRuntime, TW08AudioCatalog audioCatalog)
        {
            runtime = puzzleRuntime;
            catalog = audioCatalog;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            if (runtime == null) return;
            runtime.MoveApplied += OnMove;
            runtime.LevelCompleted += OnCompleted;
            runtime.StaticDeadlockDetected += OnDeadlock;
        }

        private void OnDisable()
        {
            if (runtime == null) return;
            runtime.MoveApplied -= OnMove;
            runtime.LevelCompleted -= OnCompleted;
            runtime.StaticDeadlockDetected -= OnDeadlock;
        }

        private void OnMove(PuzzleMove move)
        {
            AudioService.Instance?.PlayOneShot(move.CrateMoved ? catalog?.PuzzlePush : catalog?.PuzzleStep, transform.position);
        }

        private void OnCompleted()
        {
            AudioService.Instance?.PlayOneShot(catalog?.PuzzleSuccess, transform.position);
        }

        private void OnDeadlock()
        {
            AudioService.Instance?.PlayOneShot(catalog?.PuzzleError, transform.position);
        }
    }
}
