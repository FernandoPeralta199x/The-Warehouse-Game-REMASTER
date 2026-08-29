using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.Motion
{
    /// <summary>
    /// Handle de um movimento em andamento. Guardar o handle permite cancelar
    /// (<see cref="Kill"/>) ou saltar para o estado final (<see cref="Complete"/>).
    /// </summary>
    public sealed class MotionHandle
    {
        internal Coroutine Routine;
        internal Action FinalState;
        internal bool Finished;

        /// <summary>True enquanto o movimento ainda estiver rodando.</summary>
        public bool IsPlaying => !Finished;

        /// <summary>Interrompe onde está, sem aplicar o estado final.</summary>
        public void Kill()
        {
            if (Finished)
            {
                return;
            }

            Finished = true;
            UIMotion.StopRoutine(this);
        }

        /// <summary>Interrompe e aplica imediatamente o estado final.</summary>
        public void Complete()
        {
            if (Finished)
            {
                return;
            }

            Finished = true;
            UIMotion.StopRoutine(this);
            FinalState?.Invoke();
        }
    }

    /// <summary>
    /// Serviço de movimento de interface do The Warehouse Nº 08.
    ///
    /// Tudo roda em <see cref="Time.unscaledDeltaTime"/> de propósito: menus,
    /// HUD e narrativa precisam continuar animando com o jogo pausado.
    ///
    /// O runner é um objeto persistente criado sob demanda, então qualquer cena
    /// pode animar sem precisar declarar dependência. Todo alvo é revalidado a
    /// cada frame — trocar de cena no meio de um tween não gera exceção.
    /// </summary>
    public static class UIMotion
    {
        private sealed class Runner : MonoBehaviour
        {
            private void OnDestroy()
            {
                if (instance == this)
                {
                    instance = null;
                }
            }
        }

        private static Runner instance;
        private static readonly List<MotionHandle> Active = new();

        private static Runner Host
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                GameObject host = new("TW08 UI Motion") { hideFlags = HideFlags.HideAndDontSave };
                instance = host.AddComponent<Runner>();
                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(host);
                }

                return instance;
            }
        }

        /// <summary>Interrompe todo movimento em andamento (troca de cena brusca, por exemplo).</summary>
        public static void KillAll()
        {
            foreach (MotionHandle handle in Active.ToArray())
            {
                handle.Kill();
            }

            Active.Clear();
        }

        // ----------------------------------------------------------- Fades --

        public static MotionHandle FadeTo(
            CanvasGroup group, float alpha, float duration, Ease ease = Ease.OutQuad, float delay = 0f)
        {
            if (group == null)
            {
                return Completed();
            }

            float from = group.alpha;
            return Play(
                duration,
                delay,
                ease,
                t =>
                {
                    if (group != null) group.alpha = Mathf.LerpUnclamped(from, alpha, t);
                },
                () =>
                {
                    if (group != null) group.alpha = alpha;
                });
        }

        public static MotionHandle FadeTo(
            Graphic graphic, float alpha, float duration, Ease ease = Ease.OutQuad, float delay = 0f)
        {
            if (graphic == null)
            {
                return Completed();
            }

            Color from = graphic.color;
            return Play(
                duration,
                delay,
                ease,
                t =>
                {
                    if (graphic == null) return;
                    Color c = from;
                    c.a = Mathf.LerpUnclamped(from.a, alpha, t);
                    graphic.color = c;
                },
                () =>
                {
                    if (graphic == null) return;
                    Color c = graphic.color;
                    c.a = alpha;
                    graphic.color = c;
                });
        }

        public static MotionHandle ColorTo(
            Graphic graphic, Color target, float duration, Ease ease = Ease.OutQuad, float delay = 0f)
        {
            if (graphic == null)
            {
                return Completed();
            }

            Color from = graphic.color;
            return Play(
                duration,
                delay,
                ease,
                t =>
                {
                    if (graphic != null) graphic.color = Color.LerpUnclamped(from, target, t);
                },
                () =>
                {
                    if (graphic != null) graphic.color = target;
                });
        }

        // ---------------------------------------------------------- Escala --

        public static MotionHandle ScaleTo(
            Transform target, Vector3 scale, float duration, Ease ease = Ease.OutBack, float delay = 0f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector3 from = target.localScale;
            return Play(
                duration,
                delay,
                ease,
                t =>
                {
                    if (target != null) target.localScale = Vector3.LerpUnclamped(from, scale, t);
                },
                () =>
                {
                    if (target != null) target.localScale = scale;
                });
        }

        /// <summary>Aparecimento clássico: cresce de <paramref name="from"/> até a escala cheia.</summary>
        public static MotionHandle PopIn(
            Transform target, float duration = 0.32f, float from = 0.86f, float delay = 0f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector3 full = target.localScale;
            target.localScale = full * from;
            return ScaleTo(target, full, duration, Ease.OutBack, delay);
        }

        /// <summary>Pulso de destaque que volta sozinho ao tamanho original.</summary>
        public static MotionHandle Punch(
            Transform target, float strength = 0.14f, float duration = 0.3f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector3 baseScale = target.localScale;
            return Play(
                duration,
                0f,
                Ease.Linear,
                t =>
                {
                    if (target == null) return;
                    // Meia onda de seno: sobe e volta exatamente ao ponto inicial.
                    float pulse = Mathf.Sin(t * Mathf.PI) * strength;
                    target.localScale = baseScale * (1f + pulse);
                },
                () =>
                {
                    if (target != null) target.localScale = baseScale;
                });
        }

        // -------------------------------------------------------- Posições --

        public static MotionHandle MoveTo(
            RectTransform target, Vector2 position, float duration, Ease ease = Ease.OutCubic, float delay = 0f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector2 from = target.anchoredPosition;
            return Play(
                duration,
                delay,
                ease,
                t =>
                {
                    if (target != null) target.anchoredPosition = Vector2.LerpUnclamped(from, position, t);
                },
                () =>
                {
                    if (target != null) target.anchoredPosition = position;
                });
        }

        /// <summary>Entra deslizando a partir de um deslocamento, terminando no lugar de origem.</summary>
        public static MotionHandle SlideIn(
            RectTransform target, Vector2 offset, float duration = 0.38f, Ease ease = Ease.OutCubic, float delay = 0f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector2 destination = target.anchoredPosition;
            target.anchoredPosition = destination + offset;
            return MoveTo(target, destination, duration, ease, delay);
        }

        /// <summary>Tremor decrescente — usado em erro, dano e carga travada.</summary>
        public static MotionHandle Shake(
            RectTransform target, float strength = 14f, float duration = 0.4f)
        {
            if (target == null)
            {
                return Completed();
            }

            Vector2 origin = target.anchoredPosition;
            // Semente fixa por chamada mantém o tremor reproduzível dentro do frame.
            float seed = UnityEngine.Random.value * 100f;
            return Play(
                duration,
                0f,
                Ease.Linear,
                t =>
                {
                    if (target == null) return;
                    float decay = 1f - t;
                    float x = (Mathf.PerlinNoise(seed, t * 24f) - 0.5f) * 2f;
                    float y = (Mathf.PerlinNoise(seed + 37f, t * 24f) - 0.5f) * 2f;
                    target.anchoredPosition = origin + new Vector2(x, y) * (strength * decay);
                },
                () =>
                {
                    if (target != null) target.anchoredPosition = origin;
                });
        }

        // ----------------------------------------------------------- Texto --

        /// <summary>
        /// Máquina de escrever. <paramref name="charactersPerSecond"/> controla o
        /// ritmo; a narrativa usa isto e o jogador pode pular com Complete().
        /// </summary>
        public static MotionHandle Typewriter(
            Text label, string fullText, float charactersPerSecond = 42f, float delay = 0f)
        {
            if (label == null)
            {
                return Completed();
            }

            fullText ??= string.Empty;
            float duration = charactersPerSecond > 0f
                ? fullText.Length / charactersPerSecond
                : 0f;

            label.text = string.Empty;
            return Play(
                duration,
                delay,
                Ease.Linear,
                t =>
                {
                    if (label == null) return;
                    int count = Mathf.Clamp(Mathf.RoundToInt(fullText.Length * t), 0, fullText.Length);
                    label.text = fullText.Substring(0, count);
                },
                () =>
                {
                    if (label != null) label.text = fullText;
                });
        }

        /// <summary>Contador numérico animado — placar de movimentos, créditos, tempo.</summary>
        public static MotionHandle CountTo(
            Text label, int from, int to, float duration = 0.6f, string format = "{0}", Ease ease = Ease.OutCubic)
        {
            if (label == null)
            {
                return Completed();
            }

            return Play(
                duration,
                0f,
                ease,
                t =>
                {
                    if (label == null) return;
                    int value = Mathf.RoundToInt(Mathf.LerpUnclamped(from, to, t));
                    label.text = string.Format(format, value);
                },
                () =>
                {
                    if (label != null) label.text = string.Format(format, to);
                });
        }

        // ------------------------------------------------------- Sequência --

        /// <summary>Encadeia passos com espera entre eles, sem aninhar corrotinas na chamada.</summary>
        public sealed class Sequence
        {
            private readonly List<(float delay, Action step)> steps = new();

            public Sequence Then(Action step, float delayBefore = 0f)
            {
                steps.Add((Mathf.Max(0f, delayBefore), step));
                return this;
            }

            public Sequence Wait(float seconds)
            {
                steps.Add((Mathf.Max(0f, seconds), null));
                return this;
            }

            public MotionHandle Play()
            {
                MotionHandle handle = new();
                handle.FinalState = () =>
                {
                    foreach ((float _, Action step) in steps)
                    {
                        step?.Invoke();
                    }
                };

                Active.Add(handle);
                handle.Routine = Host.StartCoroutine(RunSequence(handle, steps));
                return handle;
            }
        }

        public static Sequence Chain() => new();

        // ----------------------------------------------------------- Infra --

        private static MotionHandle Completed()
        {
            return new MotionHandle { Finished = true };
        }

        private static MotionHandle Play(
            float duration, float delay, Ease ease, Action<float> apply, Action finalState)
        {
            MotionHandle handle = new() { FinalState = finalState };

            // Fora do Play Mode não existe loop de frames confiável: aplica o
            // estado final na hora para o Editor não ficar com objetos a meio caminho.
            if (!Application.isPlaying || duration <= 0f)
            {
                finalState?.Invoke();
                handle.Finished = true;
                return handle;
            }

            Active.Add(handle);
            handle.Routine = Host.StartCoroutine(RunTween(handle, duration, delay, ease, apply, finalState));
            return handle;
        }

        private static IEnumerator RunTween(
            MotionHandle handle, float duration, float delay, Ease ease, Action<float> apply, Action finalState)
        {
            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                apply(Easing.Evaluate(ease, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            finalState?.Invoke();
            handle.Finished = true;
            Active.Remove(handle);
        }

        private static IEnumerator RunSequence(
            MotionHandle handle, List<(float delay, Action step)> steps)
        {
            foreach ((float delay, Action step) in steps)
            {
                if (delay > 0f)
                {
                    yield return new WaitForSecondsRealtime(delay);
                }

                if (handle.Finished)
                {
                    yield break;
                }

                step?.Invoke();
            }

            handle.Finished = true;
            Active.Remove(handle);
        }

        internal static void StopRoutine(MotionHandle handle)
        {
            if (handle.Routine != null && instance != null)
            {
                instance.StopCoroutine(handle.Routine);
                handle.Routine = null;
            }

            Active.Remove(handle);
        }
    }
}
