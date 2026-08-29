using TW08.Core;
using TW08.Motion;
using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Tela de carregamento em estilo terminal: barra que preenche, porcentagem e
    /// linhas de log digitadas.
    ///
    /// O <see cref="SceneLoader"/> de runtime é criado sob demanda por
    /// <c>SceneLoader.TryLoadImmediate</c> e não existe na cena salva, então este
    /// apresentador o descobre sozinho em vez de depender de uma referência
    /// serializada. Ele também aparece durante a saída animada do menu, para o
    /// carregamento não ser um corte seco.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoadingScreenPresenter : MonoBehaviour
    {
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private CanvasGroup root;
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private Text statusText;
        [SerializeField] private Text percentText;
        [SerializeField] private Text cursorText;
        [SerializeField] private bool showDuringMenuTransition = true;
        [SerializeField, Min(0.5f)] private float fadeSpeed = 7f;
        [SerializeField, Min(0.1f)] private float fillSpeed = 2.6f;
        [SerializeField, Min(0.1f)] private float cursorBlinkRate = 2.4f;

        private SceneLoader bound;
        private MotionHandle statusHandle;
        private string statusShown = string.Empty;
        private float reported;
        private float shown;
        private float discoverTimer;

        /// <summary>Porcentagem no formato do terminal ("000%" a "100%"). Regra pura.</summary>
        public static string FormatPercent(float progress)
        {
            int percent = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            return percent.ToString("000") + "%";
        }

        /// <summary>Linha de log correspondente à faixa de progresso. Regra pura.</summary>
        public static string StatusFor(float progress)
        {
            float clamped = Mathf.Clamp01(progress);
            if (clamped >= 1f) return "SETOR PRONTO.";
            if (clamped >= 0.75f) return "LIGANDO ILUMINAÇÃO DE EMERGÊNCIA...";
            if (clamped >= 0.5f) return "ALINHANDO CORREDORES...";
            if (clamped >= 0.25f) return "CARREGANDO MANIFESTO DE CARGA...";
            return "INICIALIZANDO SETOR...";
        }

        public void Configure(
            CanvasGroup rootGroup,
            RectTransform fill,
            Text status,
            Text percent,
            Text cursor)
        {
            root = rootGroup;
            progressFill = fill;
            statusText = status;
            percentText = percent;
            cursorText = cursor;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            reported = 0f;
            shown = 0f;
            statusShown = string.Empty;
            ApplyFill(0f);

            if (root != null)
            {
                root.alpha = 0f;
                root.blocksRaycasts = false;
                root.interactable = false;
            }

            if (sceneLoader != null)
            {
                Bind(sceneLoader);
            }
        }

        private void OnDisable()
        {
            Unbind();
            statusHandle?.Kill();
            statusHandle = null;
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            bool transitioning = showDuringMenuTransition && MenuTransition.IsTransitioning;
            bool loading = bound != null && bound.IsLoading;
            bool visible = loading || transitioning;

            if (bound == null)
            {
                discoverTimer -= delta;
                bool alreadyVisible = root != null && root.alpha > 0.01f;
                if (discoverTimer <= 0f || alreadyVisible)
                {
                    discoverTimer = 0.1f;
                    TryDiscover();
                }
            }

            if (!loading && transitioning)
            {
                // Enquanto o menu ainda está fechando não há progresso real: a barra
                // avança até um terço para não parecer travada.
                reported = Mathf.Min(0.34f, reported + delta * 0.55f);
            }

            shown = Mathf.MoveTowards(shown, reported, fillSpeed * delta);
            ApplyFill(shown);

            if (percentText != null)
            {
                percentText.text = FormatPercent(shown);
            }

            UpdateStatus(shown);

            if (cursorText != null)
            {
                bool on = Mathf.Repeat(Time.unscaledTime * cursorBlinkRate, 2f) < 1f;
                if (cursorText.enabled != on)
                {
                    cursorText.enabled = on;
                }
            }

            if (root == null)
            {
                return;
            }

            root.alpha = Mathf.MoveTowards(root.alpha, visible ? 1f : 0f, fadeSpeed * delta);
            bool solid = root.alpha > 0.5f;
            if (root.blocksRaycasts != solid)
            {
                root.blocksRaycasts = solid;
            }
        }

        private void UpdateStatus(float progress)
        {
            if (statusText == null)
            {
                return;
            }

            string next = StatusFor(progress);
            if (string.Equals(next, statusShown, System.StringComparison.Ordinal))
            {
                return;
            }

            statusShown = next;
            statusHandle?.Complete();
            statusHandle = UIMotion.Typewriter(statusText, next, 52f);
        }

        private void ApplyFill(float progress)
        {
            if (progressFill == null)
            {
                return;
            }

            Vector2 max = progressFill.anchorMax;
            float clamped = Mathf.Clamp01(progress);
            if (Mathf.Abs(max.x - clamped) < 0.0005f)
            {
                return;
            }

            max.x = clamped;
            progressFill.anchorMax = max;
        }

        private void TryDiscover()
        {
            SceneLoader candidate = sceneLoader != null
                ? sceneLoader
                : FindFirstObjectByType<SceneLoader>();
            if (candidate != null)
            {
                Bind(candidate);
            }
        }

        private void Bind(SceneLoader loader)
        {
            if (bound == loader)
            {
                return;
            }

            Unbind();
            bound = loader;
            bound.ProgressChanged += OnProgress;
            bound.SceneLoaded += OnFinished;
            bound.SceneLoadFailed += OnFinished;
        }

        private void Unbind()
        {
            if (bound == null)
            {
                return;
            }

            bound.ProgressChanged -= OnProgress;
            bound.SceneLoaded -= OnFinished;
            bound.SceneLoadFailed -= OnFinished;
            bound = null;
        }

        private void OnProgress(float progress)
        {
            reported = Mathf.Max(reported, Mathf.Clamp01(progress));
        }

        private void OnFinished(string _)
        {
            reported = 1f;
        }
    }
}
