using TW08.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private SaveManager saveManager;

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
            masterSlider?.onValueChanged.AddListener(OnChanged);
            musicSlider?.onValueChanged.AddListener(OnChanged);
            sfxSlider?.onValueChanged.AddListener(OnChanged);
            backButton?.onClick.AddListener(Back);
            LoadValues();
        }

        private void OnDisable()
        {
            masterSlider?.onValueChanged.RemoveListener(OnChanged);
            musicSlider?.onValueChanged.RemoveListener(OnChanged);
            sfxSlider?.onValueChanged.RemoveListener(OnChanged);
            backButton?.onClick.RemoveListener(Back);
        }

        public void Back()
        {
            if (!string.IsNullOrWhiteSpace(backScene)) SceneManager.LoadScene(backScene, LoadSceneMode.Single);
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

        private void OnChanged(float _)
        {
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
            if (masterValue != null) masterValue.text = Mathf.RoundToInt(master * 100f) + "%";
            if (musicValue != null) musicValue.text = Mathf.RoundToInt(music * 100f) + "%";
            if (sfxValue != null) sfxValue.text = Mathf.RoundToInt(sfx * 100f) + "%";
            if (!persist) return;

            PlayerPrefs.SetFloat("tw08.audio.master", master);
            PlayerPrefs.SetFloat("tw08.audio.music", music);
            PlayerPrefs.SetFloat("tw08.audio.sfx", sfx);
            PlayerPrefs.Save();
            saveManager?.UpdateAudioSettings(master, music, sfx);
        }
    }
}
