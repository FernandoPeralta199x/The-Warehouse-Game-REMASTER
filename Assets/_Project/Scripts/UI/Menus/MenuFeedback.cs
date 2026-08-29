using TW08.Motion;
using TW08.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Respostas curtas de interface: confirmar dá um pulso, recusar treme.
    ///
    /// O pulso é delegado ao <see cref="MenuFocusAnimator"/> quando existe um na
    /// hierarquia. Ele já escreve <c>localScale</c> todo frame; deixar um tween
    /// paralelo escrevendo a mesma propriedade produziria disputa de escrita.
    /// O tremor mexe em <c>anchoredPosition</c>, que ninguém mais controla, então
    /// pode ir direto pelo serviço de movimento.
    /// </summary>
    public static class MenuFeedback
    {
        public static void Click(Component target)
        {
            // Confirmar precisa soar: navegar em silêncio parece travado.
            GameAudio.Confirm();

            if (target == null)
            {
                return;
            }

            MenuFocusAnimator animator = target.GetComponentInParent<MenuFocusAnimator>();
            if (animator != null && target.transform is RectTransform rect)
            {
                animator.PlayClick(rect);
                return;
            }

            UIMotion.Punch(target.transform, 0.13f, 0.26f);
        }

        public static void Denied(Component target)
        {
            GameAudio.Denied();

            if (target == null || target.transform is not RectTransform rect)
            {
                return;
            }

            UIMotion.Shake(rect, 12f, 0.34f);
        }

        /// <summary>Destaque rápido de um texto que acabou de mudar de estado.</summary>
        public static void Flash(Graphic graphic, Color highlight, float holdDuration = 0.18f)
        {
            if (graphic == null)
            {
                return;
            }

            Color settled = graphic.color;
            graphic.color = highlight;
            UIMotion.ColorTo(graphic, settled, 0.42f, Ease.OutQuad, Mathf.Max(0f, holdDuration));
        }
    }
}
