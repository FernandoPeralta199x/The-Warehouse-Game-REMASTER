using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.Motion
{
    /// <summary>De onde o elemento entra em cena.</summary>
    public enum EntranceStyle
    {
        Fade,
        SlideUp,
        SlideDown,
        SlideLeft,
        SlideRight,
        Pop
    }

    /// <summary>
    /// Anima a entrada de um painel e, opcionalmente, escalona os filhos em
    /// cascata.
    ///
    /// Existe para que builders de cena não precisem escrever movimento à mão:
    /// basta adicionar o componente e chamar <see cref="Configure"/>. A cascata
    /// é o que dá ao menu a sensação de terminal ligando linha a linha.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIEntranceAnimator : MonoBehaviour
    {
        [SerializeField] private EntranceStyle style = EntranceStyle.SlideUp;
        [SerializeField, Min(0f)] private float duration = 0.42f;
        [SerializeField, Min(0f)] private float delay;
        [SerializeField, Min(0f)] private float distance = 48f;
        [SerializeField] private bool staggerChildren;
        [SerializeField, Min(0f)] private float staggerInterval = 0.055f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private readonly List<MotionHandle> handles = new();
        private CanvasGroup group;

        public void Configure(
            EntranceStyle entranceStyle,
            float entranceDuration = 0.42f,
            float entranceDelay = 0f,
            bool stagger = false,
            float interval = 0.055f,
            float slideDistance = 48f)
        {
            style = entranceStyle;
            duration = entranceDuration;
            delay = entranceDelay;
            staggerChildren = stagger;
            staggerInterval = interval;
            distance = slideDistance;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            // Sem isto, sair da cena no meio da cascata deixaria filhos invisíveis.
            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
        }

        /// <summary>Reexecuta a entrada — útil ao reabrir um painel sem recarregar a cena.</summary>
        public void Play()
        {
            foreach (MotionHandle handle in handles)
            {
                handle?.Kill();
            }

            handles.Clear();

            if (staggerChildren)
            {
                PlayStaggered();
                return;
            }

            handles.Add(Animate(transform, delay));
        }

        private void PlayStaggered()
        {
            EnsureGroup().alpha = 1f;

            int index = 0;
            foreach (Transform child in transform)
            {
                if (child == null || !child.gameObject.activeSelf)
                {
                    continue;
                }

                handles.Add(Animate(child, delay + index * staggerInterval));
                index++;
            }
        }

        private MotionHandle Animate(Transform target, float startDelay)
        {
            if (target is not RectTransform rect)
            {
                return UIMotion.PopIn(target, duration, 0.9f, startDelay);
            }

            CanvasGroup targetGroup = target == transform
                ? EnsureGroup()
                : EnsureGroupOn(target.gameObject);

            targetGroup.alpha = 0f;
            UIMotion.FadeTo(targetGroup, 1f, duration * 0.8f, Ease.OutQuad, startDelay);

            switch (style)
            {
                case EntranceStyle.SlideUp:
                    return UIMotion.SlideIn(rect, new Vector2(0f, -distance), duration, ease, startDelay);
                case EntranceStyle.SlideDown:
                    return UIMotion.SlideIn(rect, new Vector2(0f, distance), duration, ease, startDelay);
                case EntranceStyle.SlideLeft:
                    return UIMotion.SlideIn(rect, new Vector2(distance, 0f), duration, ease, startDelay);
                case EntranceStyle.SlideRight:
                    return UIMotion.SlideIn(rect, new Vector2(-distance, 0f), duration, ease, startDelay);
                case EntranceStyle.Pop:
                    return UIMotion.PopIn(rect, duration, 0.86f, startDelay);
                default:
                    return UIMotion.FadeTo(targetGroup, 1f, duration, Ease.OutQuad, startDelay);
            }
        }

        private CanvasGroup EnsureGroup()
        {
            if (group == null)
            {
                group = EnsureGroupOn(gameObject);
            }

            return group;
        }

        private static CanvasGroup EnsureGroupOn(GameObject target)
        {
            return target.TryGetComponent(out CanvasGroup existing)
                ? existing
                : target.AddComponent<CanvasGroup>();
        }
    }
}
