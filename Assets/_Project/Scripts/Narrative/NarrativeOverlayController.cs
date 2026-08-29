using System;
using TW08.Data;
using TW08.Motion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInputSource = TW08.Input.GameInput;

namespace TW08.Narrative
{
    /// <summary>
    /// Player de cutscene do armazém.
    ///
    /// Monta a própria hierarquia de Canvas em runtime: os construtores de cena de
    /// puzzle não conhecem narrativa, então o overlay precisa existir sem wiring.
    /// Tudo o que ele anima passa por UIMotion, que roda em tempo não-escalado —
    /// por isso a cutscene funciona com o jogo pausado em timeScale 0.
    ///
    /// Degradação: sem elenco, sem retrato ou sem perfil do falante, o overlay
    /// continua exibindo o texto. Nada aqui lança por dado ausente.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NarrativeOverlayController : MonoBehaviour
    {
        private const float InputGuardSeconds = 0.12f;

        private static readonly Color Backdrop = new(0.010f, 0.015f, 0.018f, 0.86f);
        private static readonly Color PanelColor = new(0.035f, 0.050f, 0.055f, 0.97f);
        private static readonly Color PortraitFrameColor = new(0.070f, 0.095f, 0.100f, 1f);
        private static readonly Color TextPrimary = new(0.87f, 0.96f, 0.91f, 1f);
        private static readonly Color TextMuted = new(0.47f, 0.64f, 0.57f, 1f);
        private static readonly Color Amber = new(1f, 0.63f, 0.12f, 1f);
        private static readonly Color Cyan = new(0.26f, 0.84f, 0.92f, 1f);

        [SerializeField] private NarrativeService service;
        [SerializeField] private CharacterRoster roster;
        [Tooltip("Zera o timeScale enquanto a cutscene está no ar.")]
        [SerializeField] private bool pauseGameplay = true;
        [Tooltip("Desliga o GameInput da cena para o jogador não empurrar carga durante a fala.")]
        [SerializeField] private bool suspendPlayerInput = true;
        [SerializeField] private int sortingOrder = 500;

        private Canvas canvas;
        private CanvasGroup group;
        private Image portraitFront;
        private Image portraitGhost;
        private RectTransform portraitRect;
        private RectTransform speakerRect;
        private Text speakerLabel;
        private Text bodyLabel;
        private Text hintLabel;

        private Vector2 portraitHome;
        private Vector2 speakerHome;

        private MotionHandle typewriter;
        private MotionHandle overlayFade;
        private MotionHandle portraitFade;
        private MotionHandle ghostFade;
        private MotionHandle speakerPunch;
        private MotionHandle speakerSlide;
        private MotionHandle portraitSlide;
        private MotionHandle closeChain;

        private NarrativeLine currentLine;
        private string currentSpeakerId = string.Empty;
        private float lineStartedAt;
        private bool bound;
        private bool visible;

        private float previousTimeScale = 1f;
        private bool timeScaleOverridden;
        private GameInputSource suspendedInput;

        public bool IsVisible => visible;

        public void Configure(NarrativeService narrativeService, CharacterRoster characterRoster)
        {
            Unbind();
            service = narrativeService;
            roster = characterRoster;
            Bind();
        }

        private void Awake()
        {
            BuildInterface();
            ApplyHiddenState();
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
            // Sair de cena no meio de uma cutscene não pode deixar o jogo em timeScale 0.
            RestoreGameplay();
        }

        private void Bind()
        {
            if (bound || service == null)
            {
                return;
            }

            service.SequenceStarted += OnSequenceStarted;
            service.SequenceCompleted += OnSequenceCompleted;
            service.LineChanged += OnLineChanged;
            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            if (service != null)
            {
                service.SequenceStarted -= OnSequenceStarted;
                service.SequenceCompleted -= OnSequenceCompleted;
                service.LineChanged -= OnLineChanged;
            }

            bound = false;
        }

        private void Update()
        {
            if (!visible || currentLine == null || service == null)
            {
                return;
            }

            float elapsed = Time.unscaledTime - lineStartedAt;
            if (elapsed < InputGuardSeconds)
            {
                return;
            }

            if (WasSkipPressed())
            {
                service.SkipAll();
                return;
            }

            if (!WasAdvancePressed())
            {
                return;
            }

            // Padrão de gênero: o primeiro toque completa o texto, o segundo avança.
            if (typewriter != null && typewriter.IsPlaying)
            {
                typewriter.Complete();
                return;
            }

            if (elapsed < currentLine.MinimumDisplaySeconds)
            {
                return;
            }

            service.Advance();
        }

        // ----------------------------------------------------------- Eventos --

        private void OnSequenceStarted(NarrativeSequence sequence)
        {
            Show();
        }

        private void OnSequenceCompleted(NarrativeSequence sequence)
        {
            // A fila pode emendar outra sequência no mesmo instante (abertura +
            // entrada de setor). Fechar aqui causaria um piscada entre as duas.
            if (service != null && service.HasPending)
            {
                return;
            }

            Hide();
        }

        private void OnLineChanged(NarrativeLine line)
        {
            if (line == null)
            {
                return;
            }

            ShowLine(line);
        }

        // -------------------------------------------------------- Exibição --

        private void Show()
        {
            if (visible)
            {
                return;
            }

            visible = true;
            closeChain?.Kill();
            closeChain = null;

            if (canvas != null)
            {
                canvas.enabled = true;
            }

            if (group != null)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                group.alpha = 0f;
                overlayFade?.Kill();
                overlayFade = UIMotion.FadeTo(group, 1f, 0.28f, Ease.OutQuad);
            }

            SuspendGameplay();
        }

        private void Hide()
        {
            if (!visible)
            {
                return;
            }

            visible = false;
            currentLine = null;
            currentSpeakerId = string.Empty;
            typewriter?.Kill();
            typewriter = null;

            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                overlayFade?.Kill();
                overlayFade = UIMotion.FadeTo(group, 0f, 0.30f, Ease.InQuad);
            }

            closeChain = UIMotion.Chain()
                .Wait(0.32f)
                .Then(FinishClose)
                .Play();

            RestoreGameplay();
        }

        private void FinishClose()
        {
            // O encadeamento roda em um objeto persistente: este componente pode
            // ter sido destruído junto com a cena antes do passo final.
            if (this == null || visible)
            {
                return;
            }

            ApplyHiddenState();
        }

        private void ApplyHiddenState()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

        private void ShowLine(NarrativeLine line)
        {
            currentLine = line;
            lineStartedAt = Time.unscaledTime;

            CharacterProfile profile = ResolveProfile(line.SpeakerId);
            ApplySpeaker(line.SpeakerId, profile);

            if (bodyLabel != null)
            {
                bodyLabel.color = ToneColor(line.Tone);
                typewriter?.Kill();

                float speed = service != null && service.Current != null
                    ? service.Current.ResolveSpeed(line)
                    : 38f;
                typewriter = UIMotion.Typewriter(bodyLabel, line.Text, speed * ToneSpeedScale(line.Tone));
            }

            if (hintLabel != null)
            {
                hintLabel.text = "ESPAÇO / CLIQUE  AVANÇAR      ESC  PULAR";
            }
        }

        private void ApplySpeaker(string speakerId, CharacterProfile profile)
        {
            bool changed = !string.Equals(currentSpeakerId, speakerId, StringComparison.OrdinalIgnoreCase);
            currentSpeakerId = speakerId ?? string.Empty;

            Color accent = profile != null ? profile.UiAccent : FallbackAccent(speakerId);

            if (speakerLabel != null)
            {
                speakerLabel.text = ResolveDisplayName(speakerId, profile).ToUpperInvariant();
                speakerLabel.color = accent;

                if (changed && speakerRect != null)
                {
                    speakerSlide?.Kill();
                    speakerPunch?.Complete();
                    speakerRect.anchoredPosition = speakerHome;
                    speakerSlide = UIMotion.SlideIn(speakerRect, new Vector2(-26f, 0f), 0.26f, Ease.OutCubic);
                    speakerPunch = UIMotion.Punch(speakerRect, 0.10f, 0.26f);
                }
            }

            if (portraitFront == null)
            {
                return;
            }

            Sprite sprite = profile != null ? profile.Portrait : null;

            if (!changed)
            {
                portraitFront.sprite = sprite;
                portraitFront.enabled = sprite != null;
                return;
            }

            if (portraitGhost != null)
            {
                // Cross-fade: o retrato anterior sai por cima enquanto o novo entra.
                ghostFade?.Kill();
                portraitGhost.sprite = portraitFront.sprite;
                portraitGhost.enabled = portraitFront.sprite != null;
                portraitGhost.color = new Color(1f, 1f, 1f, portraitFront.enabled ? 1f : 0f);
                ghostFade = UIMotion.FadeTo(portraitGhost, 0f, 0.24f, Ease.OutQuad);
            }

            portraitFront.sprite = sprite;
            portraitFront.enabled = sprite != null;
            portraitFront.color = new Color(1f, 1f, 1f, 0f);

            portraitFade?.Kill();
            portraitFade = UIMotion.FadeTo(portraitFront, 1f, 0.26f, Ease.OutQuad);

            if (portraitRect != null)
            {
                portraitSlide?.Kill();
                portraitRect.anchoredPosition = portraitHome;
                portraitSlide = UIMotion.SlideIn(portraitRect, new Vector2(0f, -34f), 0.30f, Ease.OutCubic);
            }
        }

        private CharacterProfile ResolveProfile(string speakerId)
        {
            if (roster == null || string.IsNullOrWhiteSpace(speakerId))
            {
                return null;
            }

            return roster.Find(speakerId);
        }

        private static string ResolveDisplayName(string speakerId, CharacterProfile profile)
        {
            if (profile != null)
            {
                return profile.DisplayName;
            }

            if (string.IsNullOrWhiteSpace(speakerId))
            {
                return string.Empty;
            }

            switch (speakerId.Trim().ToLowerInvariant())
            {
                case "sistema": return "Sistema N-8";
                case "terminal": return "Terminal N-8";
                case "elias": return "Elias";
                case "john": return "John Miller";
                case "duda": return "Maria Eduarda \"Duda\"";
                case "robert": return "Robert \"Big Rob\"";
                default: return speakerId.Trim();
            }
        }

        private static Color FallbackAccent(string speakerId)
        {
            if (string.IsNullOrWhiteSpace(speakerId))
            {
                return TextMuted;
            }

            switch (speakerId.Trim().ToLowerInvariant())
            {
                case "sistema":
                case "terminal":
                    return Cyan;
                default:
                    return Amber;
            }
        }

        private static Color ToneColor(NarrativeTone tone)
        {
            switch (tone)
            {
                case NarrativeTone.Sistema: return new Color(0.55f, 0.85f, 0.92f, 1f);
                case NarrativeTone.Memoria: return new Color(0.79f, 0.85f, 0.95f, 1f);
                case NarrativeTone.Tenso: return new Color(0.98f, 0.74f, 0.56f, 1f);
                case NarrativeTone.Seco: return new Color(0.80f, 0.88f, 0.83f, 1f);
                default: return TextPrimary;
            }
        }

        /// <summary>A automação digita rápido e sem alma; a memória da Duda arrasta.</summary>
        private static float ToneSpeedScale(NarrativeTone tone)
        {
            switch (tone)
            {
                case NarrativeTone.Sistema: return 1.35f;
                case NarrativeTone.Memoria: return 0.82f;
                case NarrativeTone.Tenso: return 0.92f;
                default: return 1f;
            }
        }

        // ----------------------------------------------------------- Input --

        private static bool WasAdvancePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.spaceKey.wasPressedThisFrame
                 || keyboard.enterKey.wasPressedThisFrame
                 || keyboard.numpadEnterKey.wasPressedThisFrame
                 || keyboard.eKey.wasPressedThisFrame))
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null
                   && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame);
        }

        private static bool WasSkipPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
        }

        // ------------------------------------------------------- Gameplay --

        private void SuspendGameplay()
        {
            if (pauseGameplay && !timeScaleOverridden)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                timeScaleOverridden = true;
            }

            if (!suspendPlayerInput || suspendedInput != null)
            {
                return;
            }

            // O Input System não respeita timeScale: sem desligar o GameInput o
            // jogador continuaria empurrando carga por trás da cutscene.
            GameInputSource input = FindFirstObjectByType<GameInputSource>();
            if (input != null && input.enabled)
            {
                input.enabled = false;
                suspendedInput = input;
            }
        }

        private void RestoreGameplay()
        {
            if (timeScaleOverridden)
            {
                Time.timeScale = previousTimeScale;
                timeScaleOverridden = false;
            }

            if (suspendedInput != null)
            {
                suspendedInput.enabled = true;
                suspendedInput = null;
            }
        }

        // ---------------------------------------------------- Construção --

        private void BuildInterface()
        {
            if (canvas != null)
            {
                return;
            }

            Font font = ResolveFont();

            GameObject canvasObject = new(
                "Narrative Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            group = canvasObject.GetComponent<CanvasGroup>();

            Image backdrop = CreateImage(canvasObject.transform, "Backdrop", Backdrop);
            Stretch(backdrop.rectTransform);
            backdrop.raycastTarget = true;

            Image panel = CreateImage(canvasObject.transform, "Dialogue Panel", PanelColor);
            Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1560f, 330f), new Vector2(0f, 70f));

            Image accentBar = CreateImage(panel.transform, "Accent", Cyan);
            Place(accentBar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1560f, 3f), Vector2.zero);
            accentBar.raycastTarget = false;

            Image frame = CreateImage(panel.transform, "Portrait Frame", PortraitFrameColor);
            Place(frame.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(248f, 248f), new Vector2(152f, 6f));
            frame.raycastTarget = false;

            portraitGhost = CreateImage(frame.transform, "Portrait Ghost", new Color(1f, 1f, 1f, 0f));
            Place(portraitGhost.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(228f, 228f), Vector2.zero);
            portraitGhost.raycastTarget = false;
            portraitGhost.preserveAspect = true;
            portraitGhost.enabled = false;

            portraitFront = CreateImage(frame.transform, "Portrait", new Color(1f, 1f, 1f, 0f));
            portraitRect = portraitFront.rectTransform;
            Place(portraitRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(228f, 228f), Vector2.zero);
            portraitFront.raycastTarget = false;
            portraitFront.preserveAspect = true;
            portraitFront.enabled = false;
            portraitHome = portraitRect.anchoredPosition;

            speakerLabel = CreateText(panel.transform, "Speaker", string.Empty, font, 27, Amber, TextAnchor.MiddleLeft);
            speakerRect = speakerLabel.rectTransform;
            Place(speakerRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1160f, 42f), new Vector2(300f, -26f));
            speakerHome = speakerRect.anchoredPosition;

            bodyLabel = CreateText(panel.transform, "Body", string.Empty, font, 25, TextPrimary, TextAnchor.UpperLeft);
            Place(bodyLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1200f, 196f), new Vector2(300f, -78f));
            bodyLabel.verticalOverflow = VerticalWrapMode.Overflow;

            hintLabel = CreateText(panel.transform, "Hint", string.Empty, font, 15, TextMuted, TextAnchor.MiddleRight);
            Place(hintLabel.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(760f, 30f), new Vector2(-34f, 22f));
        }

        /// <summary>
        /// A fonte embutida pode não sobreviver ao stripping de um build: sem a
        /// cascata de fallback o painel abriria com o texto invisível.
        /// </summary>
        private static Font ResolveFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Font.CreateDynamicFontFromOSFont(
                new[] { "Consolas", "Arial", "Liberation Sans", "DejaVu Sans" }, 24);
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            Transform parent, string name, string value, Font font, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
