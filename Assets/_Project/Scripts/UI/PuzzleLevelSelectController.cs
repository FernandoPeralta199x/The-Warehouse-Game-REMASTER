using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class PuzzleLevelSelectController : MonoBehaviour
    {
        [SerializeField] private PuzzleCampaignDefinition campaign;
        [SerializeField] private List<Button> levelButtons = new();
        [SerializeField] private Text operatorText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private bool bound;

        public void Configure(
            PuzzleCampaignDefinition campaignDefinition,
            IEnumerable<Button> buttons,
            Text operatorLabel,
            Button back,
            string backSceneName)
        {
            Unbind();
            campaign = campaignDefinition;
            levelButtons = buttons?.Where(button => button != null).ToList() ?? new List<Button>();
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
            if (!string.IsNullOrWhiteSpace(backScene))
            {
                SceneManager.LoadScene(backScene, LoadSceneMode.Single);
            }
        }

        private void Bind()
        {
            if (bound)
            {
                return;
            }

            for (int i = 0; i < levelButtons.Count; i++)
            {
                int captured = i;
                levelButtons[i].onClick.AddListener(() => LoadLevel(captured));
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

            // Runtime-created delegates cannot be removed one-by-one without retaining each delegate.
            // Removing all runtime listeners is safe because generated buttons do not carry gameplay listeners.
            foreach (Button button in levelButtons)
            {
                button?.onClick.RemoveAllListeners();
            }

            backButton?.onClick.RemoveListener(Back);
            bound = false;
        }

        private void LoadLevel(int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Levels.Count || !PuzzleProgressStore.IsUnlocked(campaign, index))
            {
                return;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            if (entry == null || string.IsNullOrWhiteSpace(entry.SceneName))
            {
                Debug.LogError($"TW08 campaign entry {index} has no scene name.");
                return;
            }

            SceneManager.LoadScene(entry.SceneName, LoadSceneMode.Single);
        }

        private void Refresh()
        {
            if (operatorText != null)
            {
                operatorText.text = "OPERADOR // " + Core.CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }

            if (campaign == null)
            {
                return;
            }

            for (int i = 0; i < levelButtons.Count; i++)
            {
                Button button = levelButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasEntry = i < campaign.Levels.Count && campaign.Levels[i] != null && campaign.Levels[i].Level != null;
                bool unlocked = hasEntry && PuzzleProgressStore.IsUnlocked(campaign, i);
                button.interactable = unlocked;

                Text label = button.GetComponentInChildren<Text>();
                if (label == null)
                {
                    continue;
                }

                if (!hasEntry)
                {
                    label.text = "--";
                    continue;
                }

                PuzzleLevelDefinition level = campaign.Levels[i].Level;
                int medal = PuzzleProgressStore.GetMedal(level.LevelId);
                int best = PuzzleProgressStore.GetBestMoves(level.LevelId);
                string suffix = unlocked
                    ? (best > 0 ? $"\nBEST {best:000} // M{medal}" : "\nROTA DISPONÍVEL")
                    : "\nBLOQUEADO";
                label.text = $"{i + 1:00} // {level.DisplayName.ToUpperInvariant()}{suffix}";
            }
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
