using System.Collections.Generic;
using TW08.Motion;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI.Menus
{
    /// <summary>
    /// Entrada em cascata de uma tela de menu: o cabeçalho digita como um terminal
    /// ligando e os controles surgem linha por linha.
    ///
    /// Nunca toca no <c>CanvasGroup</c> do próprio shell — esse pertence ao
    /// <see cref="ProfessionalMenuPresenter"/>, que o pipeline de produção instala
    /// no mesmo objeto. Os dois se somam: o painel acende, depois as linhas entram.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuScreenAnimator : MonoBehaviour
    {
        [SerializeField] private Text eyebrowText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private List<RectTransform> cascade = new();
        [SerializeField, Min(0f)] private float startDelay = 0.06f;
        [SerializeField, Min(0f)] private float headerLead = 0.22f;
        [SerializeField, Min(0f)] private float stepInterval = 0.055f;
        [SerializeField, Min(0.05f)] private float stepDuration = 0.34f;
        [SerializeField] private float slideDistance = 44f;
        [SerializeField] private bool typewriterHeaders = true;
        [SerializeField, Min(1f)] private float charactersPerSecond = 64f;
        [SerializeField, Range(1f, 1.3f)] private float titleWeight = 1.07f;

        private readonly List<MotionHandle> handles = new();
        private readonly List<CanvasGroup> tracked = new();

        private Vector3 titleSettledScale = Vector3.one;
        private string eyebrowSource;
        private string subtitleSource;
        private bool started;

        /// <summary>Atraso do item <paramref name="index"/> na cascata. Regra pura, testável.</summary>
        public static float StepDelay(int index, float start, float interval)
        {
            return Mathf.Max(0f, start) + Mathf.Max(0, index) * Mathf.Max(0f, interval);
        }

        public void Configure(
            Text eyebrow,
            Text title,
            Text subtitle,
            IEnumerable<Component> staggered,
            bool typewriter = true,
            float delay = 0.06f,
            float interval = 0.055f,
            float distance = 44f)
        {
            eyebrowText = eyebrow;
            titleText = title;
            subtitleText = subtitle;
            typewriterHeaders = typewriter;
            startDelay = Mathf.Max(0f, delay);
            stepInterval = Mathf.Max(0f, interval);
            slideDistance = distance;

            cascade.Clear();
            if (staggered != null)
            {
                foreach (Component component in staggered)
                {
                    if (component != null && component.transform is RectTransform rect)
                    {
                        cascade.Add(rect);
                    }
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void Awake()
        {
            if (titleText != null)
            {
                titleSettledScale = titleText.rectTransform.localScale;
            }

            if (!enabled || !gameObject.activeInHierarchy)
            {
                return;
            }

            // Esconder já no Awake evita o frame em que a tela aparece inteira
            // antes da cascata começar.
            HideForEntrance();
        }

        // A entrada roda no Start, não no OnEnable: os textos do cabeçalho podem
        // ser reescritos por outros controladores durante o Awake deles.
        private void Start()
        {
            started = true;
            Play();
        }

        private void OnEnable()
        {
            if (started)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();

            // Rede de segurança: sair da cena no meio da cascata não pode deixar
            // controles invisíveis num painel que volta a ser exibido.
            foreach (CanvasGroup group in tracked)
            {
                if (group != null)
                {
                    group.alpha = 1f;
                }
            }

            tracked.Clear();
        }

        /// <summary>Reexecuta a entrada — útil ao reabrir a tela sem recarregar a cena.</summary>
        public void Play()
        {
            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
            tracked.Clear();

            CaptureHeaderSources();

            float headerDelay = startDelay;
            AnimateHeader(eyebrowText, eyebrowSource, headerDelay);

            if (titleText != null)
            {
                RectTransform rect = titleText.rectTransform;
                CanvasGroup group = Track(rect.gameObject);
                group.alpha = 0f;
                handles.Add(UIMotion.FadeTo(group, 1f, stepDuration, Ease.OutQuad, headerDelay + 0.06f));

                rect.localScale = titleSettledScale * titleWeight;
                handles.Add(UIMotion.ScaleTo(
                    rect, titleSettledScale, stepDuration + 0.14f, Ease.OutCubic, headerDelay + 0.06f));
            }

            AnimateHeader(subtitleText, subtitleSource, headerDelay + 0.14f);

            float cascadeStart = startDelay + headerLead;
            for (int i = 0; i < cascade.Count; i++)
            {
                RectTransform rect = cascade[i];
                if (rect == null)
                {
                    continue;
                }

                float delay = StepDelay(i, cascadeStart, stepInterval);
                CanvasGroup group = Track(rect.gameObject);
                group.alpha = 0f;
                handles.Add(UIMotion.FadeTo(group, 1f, stepDuration * 0.8f, Ease.OutQuad, delay));
                handles.Add(UIMotion.SlideIn(
                    rect, new Vector2(0f, -slideDistance), stepDuration, Ease.OutCubic, delay));
            }
        }

        private void AnimateHeader(Text label, string source, float delay)
        {
            if (label == null)
            {
                return;
            }

            CanvasGroup group = Track(label.gameObject);
            group.alpha = 0f;
            handles.Add(UIMotion.FadeTo(group, 1f, stepDuration * 0.7f, Ease.OutQuad, delay));

            if (typewriterHeaders && !string.IsNullOrEmpty(source))
            {
                handles.Add(UIMotion.Typewriter(label, source, charactersPerSecond, delay));
            }
        }

        private void CaptureHeaderSources()
        {
            if (eyebrowText != null && string.IsNullOrEmpty(eyebrowSource))
            {
                eyebrowSource = eyebrowText.text;
            }

            if (subtitleText != null && string.IsNullOrEmpty(subtitleSource))
            {
                subtitleSource = subtitleText.text;
            }
        }

        private void HideForEntrance()
        {
            CaptureHeaderSources();
            HideOne(eyebrowText != null ? eyebrowText.gameObject : null);
            HideOne(titleText != null ? titleText.gameObject : null);
            HideOne(subtitleText != null ? subtitleText.gameObject : null);

            foreach (RectTransform rect in cascade)
            {
                HideOne(rect != null ? rect.gameObject : null);
            }
        }

        private void HideOne(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Track(target).alpha = 0f;
        }

        private CanvasGroup Track(GameObject target)
        {
            CanvasGroup group = target.TryGetComponent(out CanvasGroup existing)
                ? existing
                : target.AddComponent<CanvasGroup>();

            if (!tracked.Contains(group))
            {
                tracked.Add(group);
            }

            return group;
        }
    }
}
