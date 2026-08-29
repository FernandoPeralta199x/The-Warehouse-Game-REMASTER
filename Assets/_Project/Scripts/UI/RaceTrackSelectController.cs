using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Race;
using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class RaceTrackSelectController : MonoBehaviour
    {
        [SerializeField] private RaceCampaignDefinition campaign;
        [SerializeField] private List<Button> trackButtons = new();
        [SerializeField] private Text operatorText;
        [SerializeField] private Text hintText;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private readonly List<bool> unlockedCache = new();
        private bool bound;

        /// <summary>Tempo no formato do placar (mm:ss.mmm). Regra pura, testável.</summary>
        public static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0f, seconds) / 60f);
            float remainder = Mathf.Max(0f, seconds) - minutes * 60f;
            return $"{minutes:00}:{remainder:00.000}";
        }

        /// <summary>Rótulo do cartão de pista. Regra pura, testável.</summary>
        public static string FormatTrackLabel(
            int index, string displayName, bool hasTrack, bool unlocked, float bestTime, int medal)
        {
            if (!hasTrack)
            {
                return "--";
            }

            string bestText = bestTime > 0f ? FormatTime(bestTime) : "--:--.---";
            string suffix = unlocked ? $"\nBEST {bestText} // M{medal}" : "\nBLOQUEADO";
            string name = string.IsNullOrWhiteSpace(displayName)
                ? $"PISTA {index + 1:00}"
                : displayName.ToUpperInvariant();
            return $"{index + 1:00} // {name}{suffix}";
        }

        public void Configure(
            RaceCampaignDefinition campaignDefinition,
            IEnumerable<Button> buttons,
            Text operatorLabel,
            Button back,
            string backSceneName,
            Text hintLabel = null)
        {
            Unbind();
            campaign = campaignDefinition;
            trackButtons = buttons?.Where(button => button != null).ToList() ?? new List<Button>();
            operatorText = operatorLabel;
            hintText = hintLabel;
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
            MenuTransition.Go(backScene, "menu de modos");
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
            if (campaign == null || index < 0 || index >= campaign.Tracks.Count)
            {
                return;
            }

            if (!IsUnlockedAt(index))
            {
                Deny(index, "PISTA BLOQUEADA // VENÇA A ANTERIOR");
                return;
            }

            RaceTrackDefinition track = campaign.Tracks[index];
            if (track == null || string.IsNullOrWhiteSpace(track.SceneName))
            {
                Debug.LogError($"TW08 race track {index} has no scene name.");
                Deny(index, "PISTA SEM DESTINO REGISTRADO");
                return;
            }

            if (MenuTransition.Go(track.SceneName, $"pista {index + 1:00}"))
            {
                ShowHint($"LIBERANDO PISTA {index + 1:00}...");
            }
        }

        private void Deny(int index, string message)
        {
            if (index >= 0 && index < trackButtons.Count)
            {
                MenuFeedback.Denied(trackButtons[index]);
            }

            ShowHint(message);
        }

        private void ShowHint(string message)
        {
            if (hintText != null)
            {
                hintText.text = message;
            }
        }

        private bool IsUnlockedAt(int index)
        {
            return index >= 0 && index < unlockedCache.Count
                ? unlockedCache[index]
                : RaceProgressStore.IsUnlocked(campaign, index);
        }

        private void Refresh()
        {
            if (operatorText != null)
            {
                operatorText.text = "PILOTO // " + CharacterSelectionState.SelectedCharacterId.ToUpperInvariant();
            }

            unlockedCache.Clear();

            if (campaign == null)
            {
                return;
            }

            int nextIndex = -1;
            for (int i = 0; i < trackButtons.Count; i++)
            {
                bool hasTrack = i < campaign.Tracks.Count && campaign.Tracks[i] != null;
                bool unlocked = hasTrack && RaceProgressStore.IsUnlocked(campaign, i);
                unlockedCache.Add(unlocked);

                if (nextIndex < 0 && unlocked && !RaceProgressStore.IsCompleted(campaign.Tracks[i].TrackId))
                {
                    nextIndex = i;
                }
            }

            for (int i = 0; i < trackButtons.Count; i++)
            {
                Button button = trackButtons[i];
                if (button == null)
                {
                    continue;
                }

                bool hasTrack = i < campaign.Tracks.Count && campaign.Tracks[i] != null;
                bool unlocked = i < unlockedCache.Count && unlockedCache[i];
                button.interactable = hasTrack;

                Text label = button.GetComponentInChildren<Text>();
                if (label == null)
                {
                    continue;
                }

                if (!hasTrack)
                {
                    label.text = FormatTrackLabel(i, null, false, false, 0f, 0);
                    label.color = PuzzleLevelSelectController.EmptyTint;
                    continue;
                }

                RaceTrackDefinition track = campaign.Tracks[i];
                float best = RaceProgressStore.GetBestTime(track.TrackId);
                int medal = RaceProgressStore.GetMedal(track.TrackId);
                label.text = FormatTrackLabel(i, track.DisplayName, true, unlocked, best, medal);
                label.color = PuzzleLevelSelectController.LabelTint(true, unlocked, medal, i == nextIndex);
            }

            ShowHint(nextIndex >= 0
                ? $"PRÓXIMA PISTA // {nextIndex + 1:00}"
                : "TODAS AS PISTAS FORAM CORRIDAS.");

            MenuFocusAnimator.RefreshAll();
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
