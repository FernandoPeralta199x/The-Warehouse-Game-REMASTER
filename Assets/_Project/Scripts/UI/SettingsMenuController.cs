using TW08.Save;
using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text masterValue;
        [SerializeField] private Text musicValue;
        [SerializeField] private Text sfxValue;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";
        [SerializeField, Min(0.5f)] private float pulseDecay = 3.2f;

        private sealed class ValuePulse
        {
            public Text Label;
            public Vector3 BaseScale;
            public Color BaseColor;
            public float Amount;
        }

        private readonly ValuePulse[] pulses = new ValuePulse[3];
        private SaveManager saveManager;

        /// <summary>Rótulo de porcentagem dos sliders. Regra pura, testável.</summary>
        public static string FormatPercent(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }

        public void Configure(
            Slider master,
            Slider music,
            Slider sfx,
            Text masterLabel,
            Text musicLabel,
            Text sfxLabel,
            Button back,
            string backSceneName)
        {
            masterSlider = master;
            musicSlider = music;
            sfxSlider = sfx;
            masterValue = masterLabel;
            musicValue = musicLabel;
            sfxValue = sfxLabel;
            backButton = back;
            backScene = backSceneName;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable()
        {
            saveManager = Object.FindFirstObjectByType<SaveManager>();
            masterSlider?.onValueChanged.AddListener(OnMasterChanged);
            musicSlider?.onValueChanged.AddListener(OnMusicChanged);
            sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
            backButton?.onClick.AddListener(Back);

            pulses[0] = CreatePulse(masterValue);
            pulses[1] = CreatePulse(musicValue);
            pulses[2] = CreatePulse(sfxValue);

            LoadValues();
        }

        private void OnDisable()
        {
            masterSlider?.onValueChanged.RemoveListener(OnMasterChanged);
            musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
            sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
            backButton?.onClick.RemoveListener(Back);

            // Um rótulo salvo no meio do pulso ficaria maior e mais claro para sempre.
            foreach (ValuePulse pulse in pulses)
            {
                if (pulse?.Label == null)
                {
                    continue;
                }

                pulse.Label.rectTransform.localScale = pulse.BaseScale;
                pulse.Label.color = pulse.BaseColor;
                pulse.Amount = 0f;
            }
        }

        private void Update()
        {
            float decay = Time.unscaledDeltaTime * pulseDecay;

            foreach (ValuePulse pulse in pulses)
            {
                if (pulse?.Label == null || pulse.Amount <= 0f)
                {
                    continue;
                }

                pulse.Amount = Mathf.Max(0f, pulse.Amount - decay);
                pulse.Label.rectTransform.localScale = pulse.BaseScale * (1f + 0.10f * pulse.Amount);
                pulse.Label.color = Color.Lerp(pulse.BaseColor, Color.white, 0.55f * pulse.Amount);
            }
        }

        public void Back()
        {
            MenuTransition.Go(backScene, "menu de modos");
        }

        private static ValuePulse CreatePulse(Text label)
        {
            if (label == null)
            {
                return null;
            }

            return new ValuePulse
            {
                Label = label,
                BaseScale = label.rectTransform.localScale,
                BaseColor = label.color,
                Amount = 0f
            };
        }

        private void LoadValues()
        {
            float master = saveManager?.Data?.masterVolume ?? PlayerPrefs.GetFloat("tw08.audio.master", 1f);
            float music = saveManager?.Data?.musicVolume ?? PlayerPrefs.GetFloat("tw08.audio.music", 0.8f);
            float sfx = saveManager?.Data?.sfxVolume ?? PlayerPrefs.GetFloat("tw08.audio.sfx", 1f);
            if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
            Apply(master, music, sfx, false);
        }

        private void OnMasterChanged(float _) => Changed(0);

        private void OnMusicChanged(float _) => Changed(1);

        private void OnSfxChanged(float _) => Changed(2);

        private void Changed(int index)
        {
            if (index >= 0 && index < pulses.Length && pulses[index] != null)
            {
                pulses[index].Amount = 1f;
            }

            Apply(
                masterSlider != null ? masterSlider.value : 1f,
                musicSlider != null ? musicSlider.value : 0.8f,
                sfxSlider != null ? sfxSlider.value : 1f,
                true);
        }

        private void Apply(float master, float music, float sfx, bool persist)
        {
            master = Mathf.Clamp01(master);
            music = Mathf.Clamp01(music);
            sfx = Mathf.Clamp01(sfx);
            AudioListener.volume = master;
            if (masterValue != null) masterValue.text = FormatPercent(master);
            if (musicValue != null) musicValue.text = FormatPercent(music);
            if (sfxValue != null) sfxValue.text = FormatPercent(sfx);
            if (!persist) return;

            PlayerPrefs.SetFloat("tw08.audio.master", master);
            PlayerPrefs.SetFloat("tw08.audio.music", music);
            PlayerPrefs.SetFloat("tw08.audio.sfx", sfx);
            PlayerPrefs.Save();
            saveManager?.UpdateAudioSettings(master, music, sfx);
        }
    }
}
