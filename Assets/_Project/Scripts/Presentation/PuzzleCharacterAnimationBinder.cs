using TW08.Puzzle;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PuzzleCharacterAnimationBinder : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private DirectionalSpriteAnimator animator;

        private bool bound;

        public void Configure(PuzzleRuntime puzzleRuntime, DirectionalSpriteAnimator spriteAnimator)
        {
            Unbind();
            runtime = puzzleRuntime;
            animator = spriteAnimator;
            Bind();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (bound || runtime == null || animator == null)
            {
                return;
            }

            runtime.MoveApplied += OnMoveApplied;
            runtime.MoveUndone += OnMoveUndone;
            runtime.MoveRedone += OnMoveApplied;
            runtime.Initialized += OnInitialized;
            runtime.LevelRestarted += OnInitialized;
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
                runtime.MoveApplied -= OnMoveApplied;
                runtime.MoveUndone -= OnMoveUndone;
                runtime.MoveRedone -= OnMoveApplied;
                runtime.Initialized -= OnInitialized;
                runtime.LevelRestarted -= OnInitialized;
            }

            bound = false;
        }

        private void OnMoveApplied(PuzzleMove move)
        {
            GridCoordinate delta = move.PlayerTo - move.PlayerFrom;
            animator.PlayStep(delta.ToVector2Int());
        }

        private void OnMoveUndone(PuzzleMove move)
        {
            GridCoordinate delta = move.PlayerFrom - move.PlayerTo;
            animator.PlayStep(delta.ToVector2Int());
        }

        private void OnInitialized()
        {
            animator.ApplyIdle();
        }
    }
}
