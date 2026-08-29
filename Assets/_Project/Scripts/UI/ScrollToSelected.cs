using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Mantém o item selecionado (teclado/gamepad) visível dentro de um ScrollRect,
    /// com rolagem suave e uma margem de folga para o cartão nunca encostar na borda.
    ///
    /// O amortecimento é exponencial em tempo não-escalado: a velocidade percebida
    /// não muda com o frame rate nem com o jogo pausado.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ScrollToSelected : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float scrollSpeed = 15f;
        [SerializeField, Min(0f)] private float edgeMargin = 26f;

        private ScrollRect scrollRect;
        private GameObject lastSelected;

        /// <summary>
        /// Topo desejado da viewport para deixar o item inteiro visível.
        /// Devolve <paramref name="viewTop"/> quando já está enquadrado. Regra pura.
        /// </summary>
        public static float ComputeDesiredTop(
            float itemCenter, float itemHalf, float viewTop, float viewHeight, float margin)
        {
            float itemTop = itemCenter - itemHalf - margin;
            float itemBottom = itemCenter + itemHalf + margin;

            if (itemTop < viewTop)
            {
                return itemTop;
            }

            if (itemBottom > viewTop + viewHeight)
            {
                return itemBottom - viewHeight;
            }

            return viewTop;
        }

        /// <summary>Converte um topo em pixels para a posição normalizada do ScrollRect.</summary>
        public static float NormalizedFromTop(float desiredTop, float scrollable)
        {
            return 1f - Mathf.Clamp01(desiredTop / Mathf.Max(1f, scrollable));
        }

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
                lastSelected = null;
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
            float desiredTop = ComputeDesiredTop(itemCenterY, itemHalf, viewTop, viewHeight, edgeMargin);

            if (Mathf.Approximately(desiredTop, viewTop) && selected == lastSelected)
            {
                lastSelected = selected;
                return;
            }

            float targetNormalized = NormalizedFromTop(desiredTop, scrollable);
            float k = 1f - Mathf.Exp(-scrollSpeed * Time.unscaledDeltaTime);
            float next = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetNormalized, k);

            // Encostar no valor final evita ficar rolando frações de pixel para sempre.
            if (Mathf.Abs(next - targetNormalized) < 0.0008f)
            {
                next = targetNormalized;
            }

            scrollRect.verticalNormalizedPosition = next;
            lastSelected = selected;
        }
    }
}
