using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DirectionalSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private DirectionalSpriteSet spriteSet;
        [SerializeField] private FacingDirection facing = FacingDirection.Down;
        [SerializeField, Min(0.04f)] private float stepDuration = 0.14f;

        private Coroutine activeRoutine;

        public FacingDirection Facing => facing;
        public DirectionalSpriteSet SpriteSet => spriteSet;

        public void Configure(SpriteRenderer renderer, DirectionalSpriteSet set)
        {
            targetRenderer = renderer != null ? renderer : GetComponent<SpriteRenderer>();
            spriteSet = set;
            ApplyIdle();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            ApplyIdle();
        }

        private void OnDisable()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }
        }

        public void SetFacing(Vector2Int direction)
        {
            facing = FacingDirectionUtility.FromDelta(direction, facing);
            ApplyIdle();
        }

        public void PlayStep(Vector2Int direction)
        {
            facing = FacingDirectionUtility.FromDelta(direction, facing);

            if (!isActiveAndEnabled || targetRenderer == null || spriteSet == null)
            {
                ApplyIdle();
                return;
            }

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(AnimateStep());
        }

        public void ApplyIdle()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (targetRenderer == null || spriteSet == null)
            {
                return;
            }

            Sprite idle = spriteSet.GetIdle(facing);
            if (idle != null)
            {
                targetRenderer.sprite = idle;
            }
        }

        private IEnumerator AnimateStep()
        {
            IReadOnlyList<Sprite> frames = spriteSet.GetWalk(facing);
            if (frames == null || frames.Count == 0)
            {
                ApplyIdle();
                activeRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            float frameDuration = 1f / spriteSet.FramesPerSecond;
            int frameIndex = 0;

            while (elapsed < stepDuration)
            {
                Sprite frame = frames[frameIndex % frames.Count];
                if (frame != null)
                {
                    targetRenderer.sprite = frame;
                }

                float frameElapsed = 0f;
                while (frameElapsed < frameDuration && elapsed < stepDuration)
                {
                    float delta = Time.deltaTime;
                    frameElapsed += delta;
                    elapsed += delta;
                    yield return null;
                }

                frameIndex++;
            }

            ApplyIdle();
            activeRoutine = null;
        }
    }
}
