using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Data;
using TW08.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class CharacterSelectController : MonoBehaviour
    {
        [SerializeField] private CharacterRoster roster;
        [SerializeField] private Text nameText;
        [SerializeField] private Text roleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private List<CharacterProfile> characters = new();
        private int index;

        public CharacterProfile Current => characters.Count > 0 && index >= 0 && index < characters.Count
            ? characters[index]
            : null;

        public void Configure(
            CharacterRoster characterRoster,
            Text characterName,
            Text characterRole,
            Text description,
            Text status,
            Image portrait,
            Button previous,
            Button next,
            Button confirm,
            Button back,
            string backSceneName)
        {
            roster = characterRoster;
            nameText = characterName;
            roleText = characterRole;
            descriptionText = description;
            statusText = status;
            portraitImage = portrait;
            previousButton = previous;
            nextButton = next;
            confirmButton = confirm;
            backButton = back;
            backScene = backSceneName;
            MarkDirtyInEditor();
        }

        private void OnEnable()
        {
            previousButton?.onClick.AddListener(Previous);
            nextButton?.onClick.AddListener(Next);
            confirmButton?.onClick.AddListener(Confirm);
            backButton?.onClick.AddListener(Back);
            BuildList();
        }

        private void OnDisable()
        {
            previousButton?.onClick.RemoveListener(Previous);
            nextButton?.onClick.RemoveListener(Next);
            confirmButton?.onClick.RemoveListener(Confirm);
            backButton?.onClick.RemoveListener(Back);
        }

        public void Previous()
        {
            if (characters.Count == 0) return;
            index = (index - 1 + characters.Count) % characters.Count;
            Refresh();
        }

        public void Next()
        {
            if (characters.Count == 0) return;
            index = (index + 1) % characters.Count;
            Refresh();
        }

        public void Confirm()
        {
            CharacterProfile profile = Current;
            if (profile == null || (!profile.PuzzleEnabled && !profile.RaceEnabled))
            {
                return;
            }

            CharacterSelectionState.Select(profile);
            Object.FindFirstObjectByType<SaveManager>()?.SelectCharacter(profile.CharacterId);
            Refresh();
        }

        public void Back()
        {
            if (!string.IsNullOrWhiteSpace(backScene))
            {
                SceneManager.LoadScene(backScene, LoadSceneMode.Single);
            }
        }

        private void BuildList()
        {
            characters = roster != null
                ? roster.Characters.Where(character => character != null).ToList()
                : new List<CharacterProfile>();

            string selected = CharacterSelectionState.SelectedCharacterId;
            int selectedIndex = characters.FindIndex(character => character.CharacterId == selected);
            index = selectedIndex >= 0 ? selectedIndex : 0;
            Refresh();
        }

        private void Refresh()
        {
            CharacterProfile profile = Current;
            if (profile == null)
            {
                if (nameText != null) nameText.text = "OPERADOR INDISPONÍVEL";
                if (confirmButton != null) confirmButton.interactable = false;
                return;
            }

            if (nameText != null) nameText.text = profile.DisplayName.ToUpperInvariant();
            if (roleText != null) roleText.text = profile.Role.ToUpperInvariant();
            if (descriptionText != null) descriptionText.text = profile.Description;
            if (portraitImage != null)
            {
                portraitImage.sprite = profile.Portrait;
                portraitImage.enabled = profile.Portrait != null;
                portraitImage.preserveAspect = true;
            }

            bool playable = profile.PuzzleEnabled || profile.RaceEnabled;
            if (confirmButton != null) confirmButton.interactable = playable;
            if (statusText != null)
            {
                if (!playable) statusText.text = "NPC // OFICINA N-8";
                else if (profile.CharacterId == CharacterSelectionState.SelectedCharacterId) statusText.text = "OPERADOR ATIVO";
                else statusText.text = "DISPONÍVEL";
            }
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
