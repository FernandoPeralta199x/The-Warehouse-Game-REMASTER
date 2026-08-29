using System;
using System.Collections.Generic;
using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Hud
{
    /// <summary>
    /// Efeitos curtos e repetidos do HUD.
    ///
    /// Todo efeito aqui devolve um <see cref="MotionHandle"/> e assume que o
    /// chamador guarda um handle por efeito: reiniciar um pulso sem encerrar o
    /// anterior faz a escala de origem ser capturada no meio do movimento e o
    /// elemento nunca volta ao tamanho certo.
    /// </summary>
    public static class HudFx
    {
        /// <summary>Acende a cor e volta suavemente ao tom de repouso.</summary>
        public static MotionHandle Flash(Graphic graphic, Color flashColor, Color restColor, float duration = 0.24f)
        {
            if (graphic != null)
            {
                graphic.color = flashColor;
            }

            return UIMotion.ColorTo(graphic, restColor, duration, Ease.OutQuad);
        }

        /// <summary>Entrada por transparência: começa apagado e acende no lugar.</summary>
        public static MotionHandle FadeInFrom(Graphic graphic, Color restColor, float duration = 0.28f, float delay = 0f)
        {
            if (graphic != null)
            {
                graphic.color = HudPalette.WithAlpha(restColor, 0f);
            }

            return UIMotion.FadeTo(graphic, 1f, duration, Ease.OutQuad, delay);
        }

        /// <summary>
        /// Pulso seguro: encerra o pulso anterior antes de começar outro, para a
        /// escala de repouso nunca ser capturada com o elemento inflado.
        /// </summary>
        public static void Punch(ref MotionHandle handle, Transform target, float strength = 0.14f, float duration = 0.28f)
        {
            if (target == null)
            {
                return;
            }

            handle?.Complete();
            handle = UIMotion.Punch(target, strength, duration);
        }

        /// <summary>Aparecimento com escala, sempre partindo da escala cheia conhecida.</summary>
        public static void PopIn(ref MotionHandle handle, Transform target, float duration = 0.32f, float from = 0.84f, float delay = 0f)
        {
            if (target == null)
            {
                return;
            }

            handle?.Complete();
            target.localScale = Vector3.one;
            handle = UIMotion.PopIn(target, duration, from, delay);
        }

        /// <summary>Tremor de alerta, encerrando o tremor anterior para não somar deslocamento.</summary>
        public static void Shake(ref MotionHandle handle, RectTransform target, float strength = 12f, float duration = 0.42f)
        {
            if (target == null)
            {
                return;
            }

            handle?.Complete();
            handle = UIMotion.Shake(target, strength, duration);
        }

        /// <summary>Aplica o estado final e solta o handle — uso obrigatório em OnDisable.</summary>
        public static void Finish(ref MotionHandle handle)
        {
            handle?.Complete();
            handle = null;
        }

        /// <summary>Descarta o handle sem aplicar o estado final.</summary>
        public static void Abort(ref MotionHandle handle)
        {
            handle?.Kill();
            handle = null;
        }

        public static void FinishAll(List<MotionHandle> handles)
        {
            if (handles == null)
            {
                return;
            }

            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
        }

        /// <summary>
        /// Descarta a lista sem aplicar estados finais. Usar antes de remontar
        /// uma tela: concluir uma sequência aqui dispararia os passos atrasados
        /// que a nova apresentação vai refazer do zero.
        /// </summary>
        public static void AbortAll(List<MotionHandle> handles)
        {
            if (handles == null)
            {
                return;
            }

            foreach (MotionHandle handle in handles)
            {
                handle?.Kill();
            }

            handles.Clear();
        }

        /// <summary>
        /// Guarda um handle descartável, limpando os já encerrados. Sem essa
        /// poda a lista de efeitos de um HUD longo cresce sem limite.
        /// </summary>
        public static void Track(List<MotionHandle> handles, MotionHandle handle)
        {
            if (handles == null || handle == null)
            {
                return;
            }

            handles.RemoveAll(item => item == null || !item.IsPlaying);
            handles.Add(handle);
        }

        /// <summary>
        /// Executa <paramref name="action"/> depois de um atraso em tempo não
        /// escalado. Fora do Play Mode roda na hora: builders de cena não têm
        /// laço de frames e não podem ficar com passos pendentes.
        /// </summary>
        public static MotionHandle Delayed(float delay, Action action)
        {
            if (action == null)
            {
                return null;
            }

            if (!Application.isPlaying || delay <= 0f)
            {
                action.Invoke();
                return null;
            }

            return UIMotion.Chain().Wait(delay).Then(action).Play();
        }

        public static CanvasGroup EnsureGroup(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            return target.TryGetComponent(out CanvasGroup existing)
                ? existing
                : target.AddComponent<CanvasGroup>();
        }
    }
}
