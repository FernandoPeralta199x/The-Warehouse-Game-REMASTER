using System.Collections;
using UnityEngine;

namespace TW08.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class PuzzleEntityView : MonoBehaviour
    {
        [SerializeField] private string entityId = "player";
        [SerializeField] private PuzzleEntityKind kind = PuzzleEntityKind.Player;
        [SerializeField] private Vector3 worldOffset;
        [SerializeField, Min(0f)] private float moveDuration = 0.10f;

        private Coroutine moveRoutine;

        public string EntityId => entityId;
        public PuzzleEntityKind Kind => kind;

        public void Configure(string id, PuzzleEntityKind entityKind)
        {
            entityId = id;
            kind = entityKind;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public void Snap(GridCoordinate cell, float cellSize)
        {
            StopActiveMove();
            transform.position = cell.ToWorld(cellSize) + worldOffset;
        }

        public void MoveTo(GridCoordinate cell, float cellSize, bool animated)
        {
            Vector3 target = cell.ToWorld(cellSize) + worldOffset;
            if (!Application.isPlaying || !animated || moveDuration <= 0f || !isActiveAndEnabled)
            {
                Snap(cell, cellSize);
                return;
            }

            if ((transform.position - target).sqrMagnitude <= 0.000001f)
            {
                transform.position = target;
                return;
            }

            StopActiveMove();
            moveRoutine = StartCoroutine(AnimateMove(target));
        }

        private IEnumerator AnimateMove(Vector3 target)
        {
            Vector3 start = transform.position;
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / moveDuration);
                float eased = normalized * normalized * (3f - (2f * normalized));
                transform.position = Vector3.LerpUnclamped(start, target, eased);
                yield return null;
            }

            transform.position = target;
            moveRoutine = null;
        }

        private void OnDisable()
        {
            StopActiveMove();
        }

        private void StopActiveMove()
        {
            if (moveRoutine == null)
            {
                return;
            }

            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }
}
