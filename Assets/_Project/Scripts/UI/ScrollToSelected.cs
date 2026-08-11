using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Mantém o item selecionado (teclado/gamepad) visível dentro de um ScrollRect.
    /// Necessário nas telas de seleção com mais fases do que cabe na viewport.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollToSelected : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float scrollSpeed = 14f;

        private ScrollRect scrollRect;
        private GameObject lastSelected;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        private void LateUpdate()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || scrollRect == null || scrollRect.content == null)
            {
                return;
            }

            GameObject selected = eventSystem.currentSelectedGameObject;
            if (selected == null || !selected.transform.IsChildOf(scrollRect.content))
            {
                return;
            }

            var target = (RectTransform)selected.transform;
            RectTransform viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : (RectTransform)scrollRect.transform;
            RectTransform content = scrollRect.content;

            // Posição do item no espaço do content (ancorada no topo).
            float itemCenterY = Mathf.Abs(content.InverseTransformPoint(target.position).y);
            float itemHalf = target.rect.height * 0.5f;
            float viewHeight = viewport.rect.height;
            float contentHeight = content.rect.height;
            float scrollable = Mathf.Max(1f, contentHeight - viewHeight);

            float viewTop = (1f - scrollRect.verticalNormalizedPosition) * scrollable;
            float viewBottom = viewTop + viewHeight;

            float desiredTop = viewTop;
            if (itemCenterY - itemHalf < viewTop)
            {
                desiredTop = itemCenterY - itemHalf;
            }
            else if (itemCenterY + itemHalf > viewBottom)
            {
                desiredTop = itemCenterY + itemHalf - viewHeight;
            }

            if (!Mathf.Approximately(desiredTop, viewTop) || selected != lastSelected)
            {
                float targetNormalized = 1f - Mathf.Clamp01(desiredTop / scrollable);
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                    scrollRect.verticalNormalizedPosition,
                    targetNormalized,
                    Time.unscaledDeltaTime * scrollSpeed);
            }

            lastSelected = selected;
        }
    }
}
