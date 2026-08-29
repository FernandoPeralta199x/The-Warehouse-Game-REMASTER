using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Foco de menu do The Warehouse Nº 08: o item selecionado cresce, o rótulo
    /// acende com uma respiração lenta e um marcador desliza até ele.
    ///
    /// O pulso de clique é resolvido aqui dentro, e não por um tween paralelo,
    /// porque este componente escreve <c>localScale</c> todo frame — dois
    /// escritores na mesma propriedade brigariam pelo último write do frame.
    ///
    /// Cor e escala só são gravadas quando mudam de fato: a grade de fases tem 27
    /// cartões e remontar todas as malhas a cada frame custaria caro num menu parado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MenuFocusAnimator : MonoBehaviour
    {
        private const float ColorEpsilon = 0.003f;
        private const float ScaleEpsilon = 0.000001f;

        private static readonly List<MenuFocusAnimator> Instances = new();

        [SerializeField] private Transform buttonRoot;
        [SerializeField] private RectTransform marker;
        [SerializeField] private Graphic markerGraphic;
        [SerializeField] private Color markerColor = new(0.25f, 0.95f, 0.58f, 0.20f);
        [SerializeField] private Vector2 markerPadding = new(26f, 12f);
        [SerializeField, Range(1f, 1.3f)] private float selectedScale = 1.055f;
        [SerializeField, Min(1f)] private float response = 16f;
        [SerializeField, Min(1f)] private float markerResponse = 22f;
        [SerializeField, Range(0f, 1f)] private float focusGlow = 0.45f;
        [SerializeField, Range(0f, 1f)] private float backgroundGlow = 0.30f;
        [SerializeField, Range(0f, 1f)] private float disabledDim = 0.45f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 3.2f;
        [SerializeField, Range(0f, 0.4f)] private float clickPunch = 0.16f;
        [SerializeField, Min(0.5f)] private float clickDecay = 3.6f;

        private sealed class Entry
        {
            public Button Button;
            public RectTransform Rect;
            public Vector3 BaseScale;
            public Graphic Background;
            public Color BackgroundBase;
            public Color BackgroundApplied;
            public Text Label;
            public Color LabelBase;
            public Color LabelApplied;
        }

        private readonly List<Entry> entries = new();
        private RectTransform clickTarget;
        private float clickPulse;
        private bool markerVisible;

        /// <summary>Assinatura preservada: o pipeline de produção chama exatamente esta.</summary>
        public void Configure(Transform root)
        {
            buttonRoot = root;
            selectedScale = 1.055f;
            response = 16f;
            focusGlow = 0.45f;
            CacheButtons();
            MarkDirtyInEditor();
        }

        public void Configure(Transform root, RectTransform focusMarker, Graphic markerImage, Color accent)
        {
            buttonRoot = root;
            marker = focusMarker;
            markerGraphic = markerImage;
            markerColor = accent;
            selectedScale = 1.055f;
            response = 16f;
            focusGlow = 0.45f;
            CacheButtons();
            MarkDirtyInEditor();
        }

        /// <summary>
        /// Reabsorve as cores que outro controlador acabou de escrever nos rótulos
        /// (medalha, bloqueio, "EQUIPADA"). Sem isto o foco puxaria a cor de volta
        /// para o valor capturado no Awake.
        /// </summary>
        public static void RefreshAll()
        {
            foreach (MenuFocusAnimator animator in Instances)
            {
                if (animator != null)
                {
                    animator.RefreshBaseColors();
                }
            }
        }

        /// <summary>Pulso de confirmação no item recém-acionado.</summary>
        public void PlayClick(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            clickTarget = rect;
            clickPulse = 1f;
        }

        public void RefreshBaseColors()
        {
            foreach (Entry entry in entries)
            {
                if (entry.Label != null && !Close(entry.Label.color, entry.LabelApplied))
                {
                    entry.LabelBase = entry.Label.color;
                    entry.LabelApplied = entry.Label.color;
                }

                if (entry.Background != null && !Close(entry.Background.color, entry.BackgroundApplied))
                {
                    entry.BackgroundBase = entry.Background.color;
                    entry.BackgroundApplied = entry.Background.color;
                }
            }
        }

        private void Awake()
        {
            if (buttonRoot == null)
            {
                buttonRoot = transform;
            }

            CacheButtons();
        }

        private void OnEnable()
        {
            if (!Instances.Contains(this))
            {
                Instances.Add(this);
            }
        }

        private void OnDisable()
        {
            Instances.Remove(this);

            // Sair da cena com um botão ampliado deixaria o layout torto ao voltar.
            foreach (Entry entry in entries)
            {
                if (entry.Rect != null)
                {
                    entry.Rect.localScale = entry.BaseScale;
                }
            }

            markerVisible = false;
            clickPulse = 0f;
        }

        private void Update()
        {
            if (entries.Count == 0)
            {
                return;
            }

            GameObject selected = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            float t = 1f - Mathf.Exp(-response * Time.unscaledDeltaTime);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);

            if (clickPulse > 0f)
            {
                clickPulse = Mathf.Max(0f, clickPulse - Time.unscaledDeltaTime * clickDecay);
            }

            Entry focused = null;

            foreach (Entry entry in entries)
            {
                if (entry.Button == null || entry.Rect == null)
                {
                    continue;
                }

                bool usable = entry.Button.interactable;
                bool isSelected = selected != null && selected == entry.Button.gameObject;
                if (isSelected)
                {
                    focused = entry;
                }

                float punch = entry.Rect == clickTarget && clickPulse > 0f
                    ? Mathf.Sin(clickPulse * Mathf.PI) * clickPunch
                    : 0f;
                float factor = (isSelected && usable ? selectedScale : 1f) + punch;
                ApproachScale(entry.Rect, entry.BaseScale * factor, t);

                if (entry.Label != null)
                {
                    Color desired;
                    if (!usable)
                    {
                        desired = Dim(entry.LabelBase, disabledDim);
                    }
                    else if (isSelected)
                    {
                        desired = Brighten(entry.LabelBase, focusGlow * (0.7f + 0.3f * pulse));
                    }
                    else
                    {
                        desired = entry.LabelBase;
                    }

                    ApproachColor(entry.Label, desired, t, ref entry.LabelApplied);
                }

                if (entry.Background != null)
                {
                    Color desired;
                    if (!usable)
                    {
                        desired = Dim(entry.BackgroundBase, disabledDim * 0.7f);
                    }
                    else if (isSelected)
                    {
                        desired = Brighten(entry.BackgroundBase, backgroundGlow);
                    }
                    else
                    {
                        desired = entry.BackgroundBase;
                    }

                    ApproachColor(entry.Background, desired, t, ref entry.BackgroundApplied);
                }
            }

            UpdateMarker(focused, pulse);
        }

        private void UpdateMarker(Entry focused, float pulse)
        {
            if (marker == null)
            {
                return;
            }

            if (focused == null || focused.Rect == null || focused.Button == null || !focused.Button.interactable)
            {
                markerVisible = false;
                if (markerGraphic != null)
                {
                    Color faded = markerGraphic.color;
                    if (faded.a > 0.001f)
                    {
                        faded.a = Mathf.MoveTowards(faded.a, 0f, Time.unscaledDeltaTime * 4f);
                        markerGraphic.color = faded;
                    }
                }

                return;
            }

            Vector3 world = focused.Rect.TransformPoint(focused.Rect.rect.center);
            Vector2 size = focused.Rect.rect.size + markerPadding;

            if (!markerVisible)
            {
                // Primeiro foco: aparecer já no lugar, sem atravessar a tela.
                marker.position = world;
                marker.sizeDelta = size;
                markerVisible = true;
            }
            else
            {
                float m = 1f - Mathf.Exp(-markerResponse * Time.unscaledDeltaTime);
                marker.position = Vector3.Lerp(marker.position, world, m);
                marker.sizeDelta = Vector2.Lerp(marker.sizeDelta, size, m);
            }

            if (markerGraphic != null)
            {
                Color tint = markerColor;
                tint.a = markerColor.a * (0.55f + 0.45f * pulse);
                if (!Close(markerGraphic.color, tint))
                {
                    markerGraphic.color = tint;
                }
            }
        }

        private void CacheButtons()
        {
            entries.Clear();
            if (buttonRoot == null)
            {
                return;
            }

            foreach (Button button in buttonRoot.GetComponentsInChildren<Button>(true))
            {
                if (button == null || button.transform is not RectTransform rect)
                {
                    continue;
                }

                Graphic background = button.targetGraphic != null
                    ? button.targetGraphic
                    : button.GetComponent<Graphic>();
                Text label = button.GetComponentInChildren<Text>(true);

                entries.Add(new Entry
                {
                    Button = button,
                    Rect = rect,
                    BaseScale = rect.localScale,
                    Background = background,
                    BackgroundBase = background != null ? background.color : Color.white,
                    BackgroundApplied = background != null ? background.color : Color.white,
                    Label = label,
                    LabelBase = label != null ? label.color : Color.white,
                    LabelApplied = label != null ? label.color : Color.white
                });
            }
        }

        private static void ApproachScale(Transform target, Vector3 desired, float t)
        {
            Vector3 current = target.localScale;
            if ((current - desired).sqrMagnitude < ScaleEpsilon)
            {
                if (current != desired)
                {
                    target.localScale = desired;
                }

                return;
            }

            target.localScale = Vector3.Lerp(current, desired, t);
        }

        private static void ApproachColor(Graphic graphic, Color desired, float t, ref Color applied)
        {
            Color next = Color.Lerp(applied, desired, t);
            if (Close(next, desired))
            {
                next = desired;
            }

            if (Close(next, applied))
            {
                return;
            }

            applied = next;
            graphic.color = next;
        }

        private static bool Close(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < ColorEpsilon
                   && Mathf.Abs(a.g - b.g) < ColorEpsilon
                   && Mathf.Abs(a.b - b.b) < ColorEpsilon
                   && Mathf.Abs(a.a - b.a) < ColorEpsilon;
        }

        private static Color Brighten(Color color, float amount)
        {
            float k = Mathf.Clamp01(amount);
            return new Color(
                Mathf.Lerp(color.r, 1f, k),
                Mathf.Lerp(color.g, 1f, k),
                Mathf.Lerp(color.b, 1f, k),
                color.a);
        }

        private static Color Dim(Color color, float amount)
        {
            float k = 1f - Mathf.Clamp01(amount);
            return new Color(color.r * k, color.g * k, color.b * k, color.a);
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
