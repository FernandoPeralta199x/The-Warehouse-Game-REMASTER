using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Race;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RaceTrackSelectController : MonoBehaviour
    {
        [SerializeField] private RaceCampaignDefinition campaign;
        [SerializeField] private List<Button> trackButtons = new();
        [SerializeField] private Text operatorText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private bool bound;

        public void Configure(
            RaceCampaignDefinition campaignDefinition,
            IEnumerable<Button> buttons,
            Text operatorLabel,
            Button back,
            string backSceneName)
        {
            Unbind();
            campaign = campaignDefinition;
            trackButtons = buttons?.Where(button => button != null).ToList() ?? new List<Button>();
            operatorText = operatorLabel;
            backButton = back;
            backScene = backSceneName;
            Bind();
            Refresh();
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            Bind();
            Refresh();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Back()
        {
            TryLoadScene(backScene, "menu de modos");
        }

        private void Bind()
        {
            if (bound)
            {
                return;
            }

            for (int i = 0; i < trackButtons.Count; i++)
            {
                int captured = i;
                trackButtons[i].onClick.AddListener(() => LoadTrack(captured));
            }

            backButton?.onClick.AddListener(Back);
            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            foreach (Button button in trackButtons)
            {
                button?.onClick.RemoveAllListeners();
            }

            backButton?.onClick.RemoveListener(Back);
            bound = false;
        }

        private void LoadTrack(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Tracks.Count || !RaceProgressStore.IsUnlocked(campaign, index))
            {
                return;
            }

            RaceTrackDefinition track = campaign.Tracks[index];
            if (track == null || string.IsNullOrWhiteSpace(track.SceneName))
            {
                Debug.LogError($"TW08 race track {index} has no scene name.");
                return;
            }

            if (!TryLoadScene(track.SceneName, $"pista {index + 1:00}"))
            {
                if (index >= 0 && index < trackButtons.Count && trackButtons[index] != null)
                {
                    trackButtons[index].interactable = false;
                }
            }
        }

        private static bool TryLoadScene(string sceneName, string context)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"TW08 cannot load {context}: scene name is empty.");
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"TW08 cannot load {context} scene '{sceneName}' because it is not registered in the active/shared Scene List. " +
                    "Run Tools > TW08 > Production > Repair Runtime Scene Registration.");
                return false;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return true;
        }

        private void Refresh()
        {
            if (operatorText != null)
            {
                operatorText.text = "PILOTO // " + CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }

            if (campaign == null)
            {
                return;
            }

            for (int i = 0; i < trackButtons.Count; i++)
            {
                Button button = trackButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasTrack = i < campaign.Tracks.Count && campaign.Tracks[i] != null;
                bool unlocked = hasTrack && RaceProgressStore.IsUnlocked(campaign, i);
                button.interactable = unlocked;

                Text label = button.GetComponentInChildren<Text>();
                if (label == null)
                {
                    continue;
                }

                if (!hasTrack)
                {
                    label.text = "--";
                    continue;
                }

                RaceTrackDefinition track = campaign.Tracks[i];
                float best = RaceProgressStore.GetBestTime(track.TrackId);
                int medal = RaceProgressStore.GetMedal(track.TrackId);
                string bestText = best > 0f ? FormatTime(best) : "--:--.---";
                string suffix = unlocked ? $"\nBEST {bestText} // M{medal}" : "\nBLOQUEADO";
                label.text = $"{i + 1:00} // {track.DisplayName.ToUpperInvariant()}{suffix}";
            }
        }

        private static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, seconds) / 60f);
            float remainder = Mathf.Max(0f, seconds) - minutes * 60f;
            return $"{minutes:00}:{remainder:00.000}";
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