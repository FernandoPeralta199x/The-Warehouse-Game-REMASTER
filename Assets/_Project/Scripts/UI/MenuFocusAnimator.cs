using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class MenuFocusAnimator : MonoBehaviour
    {
        [SerializeField] private Transform buttonRoot;
        [SerializeField, Range(1f, 1.12f)] private float selectedScale = 1.025f;
        [SerializeField, Min(1f)] private float response = 14f;

        private readonly Dictionary<RectTransform, Vector3> baseScales = new();
        private Button[] buttons = System.Array.Empty<Button>();

        public void Configure(Transform root)
        {
            buttonRoot = root;
            CacheButtons();
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void Awake()
        {
            if (buttonRoot == null)
            {
                buttonRoot = transform;
            }
            CacheButtons();
        }

        private void Update()
        {
            if (buttons.Length == 0)
            {
                return;
            }

            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            float t = 1f - Mathf.Exp(-response * Time.unscaledDeltaTime);

            foreach (Button button in buttons)
            {
                if (button == null)
                {
                    continue;
                }

                RectTransform rect = button.transform as RectTransform;
                if (rect == null)
                {
                    continue;
                }

                if (!baseScales.TryGetValue(rect, out Vector3 baseScale))
                {
                    baseScale = rect.localScale;
                    baseScales[rect] = baseScale;
                }

                Vector3 targetScale = selected == button.gameObject && button.interactable
                    ? baseScale * selectedScale
                    : baseScale;
                rect.localScale = Vector3.Lerp(rect.localScale, targetScale, t);
            }
        }

        private void CacheButtons()
        {
            if (buttonRoot == null)
            {
                buttons = System.Array.Empty<Button>();
                return;
            }

            buttons = buttonRoot.GetComponentsInChildren<Button>(true);
            baseScales.Clear();
            foreach (Button button in buttons)
            {
                RectTransform rect = button != null ? button.transform as RectTransform : null;
                if (rect != null)
                {
                    baseScales[rect] = rect.localScale;
                }
            }
        }
    }
}
