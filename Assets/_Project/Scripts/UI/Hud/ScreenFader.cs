using System;
using System.Collections;
using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Cortina de transição entre cenas.
    ///
    /// A imagem fica sempre no topo do Canvas com <c>raycastTarget</c> desligado
    /// enquanto está transparente; só passa a bloquear cliques durante a saída,
    /// para o jogador não disparar a próxima fase duas vezes no meio do fade.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Image cover;
        [SerializeField, Min(0.05f)] private float duration = 0.42f;
        [SerializeField] private bool fadeInOnEnable = true;

        private MotionHandle handle;
        private Coroutine pending;

        /// <summary>True enquanto uma saída já foi disparada.</summary>
        public bool IsLeaving => pending != null;

        public void Configure(Image coverImage, float fadeDuration = 0.42f, bool fadeIn = true)
        {
            cover = coverImage;
            duration = Mathf.Max(0.05f, fadeDuration);
            fadeInOnEnable = fadeIn;

            if (cover != null)
            {
                cover.raycastTarget = false;
                cover.color = HudPalette.WithAlpha(Color.black, 0f);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (cover == null || !Application.isPlaying)
            {
                return;
            }

            cover.raycastTarget = false;
            if (!fadeInOnEnable)
            {
                SetAlpha(0f);
                return;
            }

            SetAlpha(1f);
            HudFx.Abort(ref handle);
            handle = UIMotion.FadeTo(cover, 0f, duration, Ease.OutQuad);
        }

        private void OnDisable()
        {
            HudFx.Abort(ref handle);
            if (pending != null)
            {
                StopCoroutine(pending);
                pending = null;
            }
        }

        /// <summary>
        /// Escurece a tela e então executa a ação. Sem cortina válida a ação
        /// roda na hora: a troca de cena nunca pode depender da animação.
        /// </summary>
        public void FadeOutThen(Action action)
        {
            if (cover == null || !Application.isPlaying || !isActiveAndEnabled)
            {
                action?.Invoke();
                return;
            }

            if (pending != null)
            {
                return;
            }

            cover.raycastTarget = true;
            HudFx.Abort(ref handle);
            handle = UIMotion.FadeTo(cover, 1f, duration, Ease.InQuad);
            pending = StartCoroutine(InvokeAfterFade(action));
        }

        private IEnumerator InvokeAfterFade(Action action)
        {
            yield return new WaitForSecondsRealtime(duration);
            pending = null;
            action?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            if (cover != null)
            {
                cover.color = HudPalette.WithAlpha(Color.black, alpha);
            }
        }
    }
}
