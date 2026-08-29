using System.Collections.Generic;
using TW08.Audio;
using TW08.Core;
using TW08.Input;
using TW08.Motion;
using TW08.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    /// <summary>
    /// Menu de pausa do turno.
    ///
    /// Monta a própria interface em runtime, como a narrativa faz, para
    /// funcionar em qualquer cena de jogo sem exigir que as 49 cenas sejam
    /// refeitas. Um painel montado à mão em cada cena sairia de sincronia na
    /// primeira mudança de layout.
    ///
    /// Tudo anima em tempo não-escalado porque o jogo está congelado enquanto
    /// o menu está aberto — animação em tempo de jogo simplesmente não rodaria.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        private const float FadeDuration = 0.22f;

        private static readonly Color Backdrop = new(0.014f, 0.019f, 0.022f, 0.88f);
        private static readonly Color Panel = new(0.035f, 0.050f, 0.055f, 0.98f);
        private static readonly Color Green = new(0.25f, 0.95f, 0.58f, 1f);
        private static readonly Color Amber = new(1f, 0.63f, 0.12f, 1f);
        private static readonly Color Cyan = new(0.26f, 0.84f, 0.92f, 1f);
        private static readonly Color TextPrimary = new(0.87f, 0.96f, 0.91f, 1f);
        private static readonly Color TextMuted = new(0.47f, 0.64f, 0.57f, 1f);

        [SerializeField] private string levelSelectScene = "TW08_PuzzleSelect";

        private GameInput input;
        private PauseService pauseService;
        private SaveManager saveManager;

        private Canvas canvas;
        private CanvasGroup group;
        private RectTransform panel;
        private GameObject firstButton;
        private readonly List<MotionHandle> handles = new();

        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Text masterValue;
        private Text musicValue;
        private Text sfxValue;

        private bool open;
        private bool settingsVisible;
        private GameObject settingsRoot;
        private GameObject buttonsRoot;

        private void Start()
        {
            input = FindFirstObjectByType<GameInput>();
            pauseService = FindFirstObjectByType<PauseService>();
            saveManager = FindFirstObjectByType<SaveManager>();

            // Sem entrada não há como abrir nem fechar o menu; melhor não existir
            // do que existir preso na tela.
            if (input == null)
            {
                enabled = false;
                return;
            }

            BuildInterface();
            input.PauseRequested += Toggle;
            SetVisible(false, instant: true);
        }

        private void OnDestroy()
        {
            if (input != null)
            {
                input.PauseRequested -= Toggle;
            }

            // Sair da cena com o menu aberto deixaria o jogo congelado.
            if (open)
            {
                ApplyPause(false);
            }
        }

        public void Toggle()
        {
            if (open)
            {
                Resume();
                return;
            }

            open = true;
            settingsVisible = false;
            ShowSettings(false);
            ApplyPause(true);
            SetVisible(true, instant: false);
            SyncSlidersFromSave();
            GameAudio.Confirm();
        }

        public void Resume()
        {
            if (!open)
            {
                return;
            }

            open = false;
            ApplyPause(false);
            SetVisible(false, instant: false);
            GameAudio.Back();
        }

        public void RestartLevel()
        {
            GameAudio.Confirm();
            ApplyPause(false);
            open = false;
            Scene active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.buildIndex, LoadSceneMode.Single);
        }

        public void ToggleSettings()
        {
            settingsVisible = !settingsVisible;
            ShowSettings(settingsVisible);
            GameAudio.Confirm();
        }

        public void BackToLevelSelect()
        {
            GameAudio.Back();
            ApplyPause(false);
            open = false;
            SceneLoader.TryLoadImmediate(levelSelectScene, "seleção de fase");
        }

        public void QuitGame()
        {
            // O progresso é gravado a cada fase concluída, mas volumes alterados
            // aqui ainda não foram para o disco: salvar antes de sair evita
            // perder a única coisa que o menu de pausa muda.
            saveManager?.Save();
            ApplyPause(false);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ApplyPause(bool paused)
        {
            if (pauseService != null)
            {
                pauseService.SetPaused(paused);
                return;
            }

            // Cenas sem PauseService ainda precisam congelar.
            Time.timeScale = paused ? 0f : 1f;
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (canvas == null)
            {
                return;
            }

            canvas.enabled = visible;
            foreach (MotionHandle handle in handles)
            {
                handle?.Kill();
            }

            handles.Clear();

            if (!visible)
            {
                if (group != null) group.alpha = 0f;
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                return;
            }

            if (instant)
            {
                if (group != null) group.alpha = 1f;
                return;
            }

            if (group != null)
            {
                group.alpha = 0f;
                handles.Add(UIMotion.FadeTo(group, 1f, FadeDuration, Ease.OutQuad));
            }

            if (panel != null)
            {
                handles.Add(UIMotion.SlideIn(panel, new Vector2(0f, -40f), 0.3f, Ease.OutCubic));
            }

            if (EventSystem.current != null && firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
        }

        private void ShowSettings(bool visible)
        {
            if (settingsRoot != null) settingsRoot.SetActive(visible);
            if (buttonsRoot != null) buttonsRoot.SetActive(!visible);

            if (visible && EventSystem.current != null && masterSlider != null)
            {
                EventSystem.current.SetSelectedGameObject(masterSlider.gameObject);
            }
            else if (!visible && EventSystem.current != null && firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton);
            }
        }

        // ------------------------------------------------------- Construção --

        private void BuildInterface()
        {
            GameObject canvasObject = new("Pause Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Acima de HUD e narrativa: o menu de pausa é a camada mais alta.
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            group = canvasObject.AddComponent<CanvasGroup>();

            Image backdrop = CreatePanel(canvasObject.transform, "Backdrop", Backdrop);
            Stretch(backdrop.rectTransform);

            Image shell = CreatePanel(canvasObject.transform, "Pause Panel", Panel);
            panel = shell.rectTransform;
            SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(700f, 620f), Vector2.zero);

            Text title = CreateText(panel, "Title", "TURNO PAUSADO", 34, TextPrimary, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(620f, 54f), new Vector2(0f, -60f));

            Text hint = CreateText(panel, "Hint", "ESC / START PARA RETOMAR", 14, TextMuted, TextAnchor.MiddleCenter);
            SetRect(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(620f, 30f), new Vector2(0f, -104f));

            BuildButtons();
            BuildSettings();

            canvasObject.AddComponent<Menus.MenuNavigationAudio>();
        }

        private void BuildButtons()
        {
            buttonsRoot = new GameObject("Buttons", typeof(RectTransform));
            buttonsRoot.transform.SetParent(panel, false);
            Stretch((RectTransform)buttonsRoot.transform);

            Button resume = CreateButton(buttonsRoot.transform, "Resume", "CONTINUAR TURNO", Green, 100f);
            Button restart = CreateButton(buttonsRoot.transform, "Restart", "REINICIAR FASE", Cyan, 25f);
            Button options = CreateButton(buttonsRoot.transform, "Options", "CONFIGURAÇÕES", Cyan, -50f);
            Button back = CreateButton(buttonsRoot.transform, "Back", "SAIR PARA SELEÇÃO", TextMuted, -125f);
            Button quit = CreateButton(buttonsRoot.transform, "Quit", "ENCERRAR TERMINAL", Amber, -200f);

            resume.onClick.AddListener(Resume);
            restart.onClick.AddListener(RestartLevel);
            options.onClick.AddListener(ToggleSettings);
            back.onClick.AddListener(BackToLevelSelect);
            quit.onClick.AddListener(QuitGame);

            firstButton = resume.gameObject;
        }

        private void BuildSettings()
        {
            settingsRoot = new GameObject("Settings", typeof(RectTransform));
            settingsRoot.transform.SetParent(panel, false);
            Stretch((RectTransform)settingsRoot.transform);

            masterSlider = CreateVolumeRow(settingsRoot.transform, "MASTER", 90f, out masterValue);
            musicSlider = CreateVolumeRow(settingsRoot.transform, "MÚSICA", 10f, out musicValue);
            sfxSlider = CreateVolumeRow(settingsRoot.transform, "EFEITOS", -70f, out sfxValue);

            masterSlider.onValueChanged.AddListener(_ => OnVolumeChanged());
            musicSlider.onValueChanged.AddListener(_ => OnVolumeChanged());
            sfxSlider.onValueChanged.AddListener(_ => OnVolumeChanged());

            Button back = CreateButton(settingsRoot.transform, "Settings Back", "VOLTAR", Green, -180f);
            back.onClick.AddListener(ToggleSettings);

            settingsRoot.SetActive(false);
        }

        private Slider CreateVolumeRow(Transform parent, string label, float y, out Text valueText)
        {
            Text caption = CreateText(parent, label + " Label", label, 18, TextPrimary, TextAnchor.MiddleLeft);
            SetRect(caption.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(180f, 34f), new Vector2(-230f, y));

            valueText = CreateText(parent, label + " Value", "100%", 18, Amber, TextAnchor.MiddleRight);
            SetRect(valueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90f, 34f), new Vector2(250f, y));

            GameObject sliderObject = new(label + " Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            SetRect((RectTransform)sliderObject.transform, new Vector2(0.5f, 0.5f), new Vector2(300f, 26f), new Vector2(30f, y));

            Image background = CreatePanel(sliderObject.transform, "Background", new Color(0.10f, 0.13f, 0.14f, 1f));
            Stretch(background.rectTransform);

            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            Stretch((RectTransform)fillArea.transform);
            Image fill = CreatePanel(fillArea.transform, "Fill", Green);
            Stretch(fill.rectTransform);

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = background;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private void OnVolumeChanged()
        {
            if (masterValue != null) masterValue.text = Mathf.RoundToInt(masterSlider.value * 100f) + "%";
            if (musicValue != null) musicValue.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";
            if (sfxValue != null) sfxValue.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";

            saveManager?.UpdateAudioSettings(masterSlider.value, musicSlider.value, sfxSlider.value);
        }

        private void SyncSlidersFromSave()
        {
            SaveGameData data = saveManager?.Data;
            if (data == null || masterSlider == null)
            {
                return;
            }

            masterSlider.SetValueWithoutNotify(data.masterVolume);
            musicSlider.SetValueWithoutNotify(data.musicVolume);
            sfxSlider.SetValueWithoutNotify(data.sfxVolume);
            OnVolumeChanged();
        }

        // ------------------------------------------------ Helpers de layout --

        private static Image CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            Transform parent, string name, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color accent, float y)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(accent.r * 0.18f, accent.g * 0.18f, accent.b * 0.18f, 0.95f);
            SetRect((RectTransform)go.transform, new Vector2(0.5f, 0.5f), new Vector2(480f, 58f), new Vector2(0f, y));

            Text text = CreateText(go.transform, "Label", label, 19, accent, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            go.AddComponent<Menus.MenuPressFeedback>();
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
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
