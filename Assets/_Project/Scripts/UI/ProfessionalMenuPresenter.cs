using UnityEngine;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class ProfessionalMenuPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField, Min(0.05f)] private float introDuration = 0.22f;
        [SerializeField, Range(0.90f, 1f)] private float introScale = 0.975f;
        [SerializeField] private Vector2 introOffset = new(0f, -18f);

        private float elapsed;
        private Vector2 targetPosition;
        private Vector3 targetScale;
        private bool initialized;

        public void Configure(CanvasGroup group, RectTransform root)
        {
            canvasGroup = group;
            contentRoot = root;
            CacheTargets();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }

            CacheTargets();
        }

        private void OnEnable()
        {
            CacheTargets();
            elapsed = 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (contentRoot != null)
            {
                contentRoot.anchoredPosition = targetPosition + introOffset;
                contentRoot.localScale = targetScale * introScale;
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, introDuration));
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = eased;
            }

            if (contentRoot != null)
            {
                contentRoot.anchoredPosition = Vector2.LerpUnclamped(targetPosition + introOffset, targetPosition, eased);
                contentRoot.localScale = Vector3.LerpUnclamped(targetScale * introScale, targetScale, eased);
            }

            if (t >= 1f && canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                enabled = false;
            }
        }

        private void CacheTargets()
        {
            if (contentRoot != null)
            {
                targetPosition = contentRoot.anchoredPosition;
                targetScale = contentRoot.localScale;
                initialized = true;
            }
        }
    }
}
