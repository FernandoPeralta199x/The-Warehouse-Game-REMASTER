using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using TW08.Data;
using TW08.Motion;
using TW08.Save;
using TW08.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace TW08.UI
{
    [DisallowMultipleComponent]
    public sealed class CharacterSelectController : MonoBehaviour
    {
        private const float PortraitSlide = 54f;

        [SerializeField] private CharacterRoster roster;
        [SerializeField] private Text nameText;
        [SerializeField] private Text roleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image portraitGhost;
        [SerializeField] private Graphic accentGraphic;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;
        [SerializeField] private string backScene = "TW08_ModeSelect";

        private readonly List<MotionHandle> handles = new();
        private List<CharacterProfile> characters = new();
        private Vector2 portraitOrigin;
        private Vector2 ghostOrigin;
        private Vector2 roleOrigin;
        private bool originsCaptured;
        private int index;

        public CharacterProfile Current => characters.Count > 0 && index >= 0 && index < characters.Count
            ? characters[index]
            : null;

        /// <summary>Rotação circular do índice do carrossel. Regra pura, testável.</summary>
        public static int CycleIndex(int current, int count, int delta)
        {
            if (count <= 0)
            {
                return 0;
            }

            int next = (current + delta) % count;
            return next < 0 ? next + count : next;
        }

        /// <summary>Texto de estado do operador. Regra pura, testável.</summary>
        public static string StatusLabel(bool playable, bool active)
        {
            if (!playable) return "NPC // OFICINA N-8";
            return active ? "OPERADOR ATIVO" : "DISPONÍVEL";
        }

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
            string backSceneName,
            Image portraitCrossFade = null,
            Graphic accent = null)
        {
            roster = characterRoster;
            nameText = characterName;
            roleText = characterRole;
            descriptionText = description;
            statusText = status;
            portraitImage = portrait;
            portraitGhost = portraitCrossFade;
            accentGraphic = accent;
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
            CaptureOrigins();
            BuildList();
        }

        private void OnDisable()
        {
            previousButton?.onClick.RemoveListener(Previous);
            nextButton?.onClick.RemoveListener(Next);
            confirmButton?.onClick.RemoveListener(Confirm);
            backButton?.onClick.RemoveListener(Back);
            StopHandles();

            // O retrato precisa voltar ao lugar: sair no meio da troca deixaria o
            // frame seguinte com a arte deslocada e meio transparente.
            RestorePortraits();
        }

        public void Previous()
        {
            if (characters.Count == 0)
            {
                MenuFeedback.Denied(previousButton);
                return;
            }

            index = CycleIndex(index, characters.Count, -1);
            Refresh(true, -1);
        }

        public void Next()
        {
            if (characters.Count == 0)
            {
                MenuFeedback.Denied(nextButton);
                return;
            }

            index = CycleIndex(index, characters.Count, 1);
            Refresh(true, 1);
        }

        public void Confirm()
        {
            CharacterProfile profile = Current;
            if (profile == null || (!profile.PuzzleEnabled && !profile.RaceEnabled))
            {
                MenuFeedback.Denied(confirmButton);
                return;
            }

            CharacterSelectionState.Select(profile);
            Object.FindFirstObjectByType<SaveManager>()?.SelectCharacter(profile.CharacterId);
            Refresh(false, 0);

            if (statusText != null)
            {
                MenuFeedback.Flash(statusText, Color.white);
            }
        }

        public void Back()
        {
            MenuTransition.Go(backScene, "menu de modos");
        }

        private void BuildList()
        {
            characters = roster != null
                ? roster.Characters.Where(character => character != null).ToList()
                : new List<CharacterProfile>();

            string selected = CharacterSelectionState.SelectedCharacterId;
            int selectedIndex = characters.FindIndex(character => character.CharacterId == selected);
            index = selectedIndex >= 0 ? selectedIndex : 0;
            Refresh(false, 0);
        }

        private void Refresh(bool animated, int direction)
        {
            CharacterProfile profile = Current;
            if (profile == null)
            {
                if (nameText != null) nameText.text = "OPERADOR INDISPONÍVEL";
                if (confirmButton != null) confirmButton.interactable = false;
                return;
            }

            StopHandles();
            CaptureOrigins();

            string displayName = profile.DisplayName.ToUpperInvariant();
            if (nameText != null)
            {
                if (animated)
                {
                    handles.Add(UIMotion.Typewriter(nameText, displayName, 46f));
                }
                else
                {
                    nameText.text = displayName;
                }
            }

            if (roleText != null)
            {
                roleText.text = profile.Role.ToUpperInvariant();
                if (animated)
                {
                    roleText.rectTransform.anchoredPosition = roleOrigin;
                    handles.Add(UIMotion.SlideIn(
                        roleText.rectTransform, new Vector2(direction * 26f, 0f), 0.3f, Ease.OutCubic));
                }
            }

            if (descriptionText != null) descriptionText.text = profile.Description;

            UpdatePortrait(profile, animated, direction);
            UpdateAccent(profile, animated);

            bool playable = profile.PuzzleEnabled || profile.RaceEnabled;
            if (confirmButton != null) confirmButton.interactable = playable;
            if (statusText != null)
            {
                statusText.text = StatusLabel(
                    playable, profile.CharacterId == CharacterSelectionState.SelectedCharacterId);
            }
        }

        private void UpdatePortrait(CharacterProfile profile, bool animated, int direction)
        {
            if (portraitImage == null)
            {
                return;
            }

            Sprite previousSprite = portraitImage.sprite;
            portraitImage.sprite = profile.Portrait;
            portraitImage.enabled = profile.Portrait != null;
            portraitImage.preserveAspect = true;

            if (!animated)
            {
                portraitImage.rectTransform.anchoredPosition = portraitOrigin;
                SetAlpha(portraitImage, 1f);
                if (portraitGhost != null)
                {
                    portraitGhost.enabled = false;
                }

                return;
            }

            float offset = direction >= 0 ? PortraitSlide : -PortraitSlide;

            if (portraitGhost != null && previousSprite != null)
            {
                portraitGhost.sprite = previousSprite;
                portraitGhost.preserveAspect = true;
                portraitGhost.enabled = true;
                portraitGhost.rectTransform.anchoredPosition = ghostOrigin;
                SetAlpha(portraitGhost, 1f);
                handles.Add(UIMotion.FadeTo(portraitGhost, 0f, 0.24f, Ease.OutQuad));
                handles.Add(UIMotion.MoveTo(
                    portraitGhost.rectTransform, ghostOrigin + new Vector2(-offset, 0f), 0.28f, Ease.InQuad));
            }

            portraitImage.rectTransform.anchoredPosition = portraitOrigin;
            SetAlpha(portraitImage, 0f);
            handles.Add(UIMotion.FadeTo(portraitImage, 1f, 0.28f, Ease.OutQuad));
            handles.Add(UIMotion.SlideIn(
                portraitImage.rectTransform, new Vector2(offset, 0f), 0.32f, Ease.OutCubic));
        }

        private void UpdateAccent(CharacterProfile profile, bool animated)
        {
            if (accentGraphic == null)
            {
                return;
            }

            Color accent = profile.UiAccent;
            accent.a = accentGraphic.color.a;

            if (animated)
            {
                handles.Add(UIMotion.ColorTo(accentGraphic, accent, 0.36f, Ease.OutQuad));
            }
            else
            {
                accentGraphic.color = accent;
            }

            if (roleText != null)
            {
                Color roleTint = profile.UiAccent;
                roleTint.a = roleText.color.a;
                if (animated)
                {
                    handles.Add(UIMotion.ColorTo(roleText, roleTint, 0.36f, Ease.OutQuad));
                }
                else
                {
                    roleText.color = roleTint;
                }
            }
        }

        private void CaptureOrigins()
        {
            if (originsCaptured)
            {
                return;
            }

            if (portraitImage != null) portraitOrigin = portraitImage.rectTransform.anchoredPosition;
            if (portraitGhost != null) ghostOrigin = portraitGhost.rectTransform.anchoredPosition;
            if (roleText != null) roleOrigin = roleText.rectTransform.anchoredPosition;
            originsCaptured = true;
        }

        private void RestorePortraits()
        {
            if (portraitImage != null)
            {
                portraitImage.rectTransform.anchoredPosition = portraitOrigin;
                SetAlpha(portraitImage, 1f);
            }

            if (portraitGhost != null)
            {
                portraitGhost.rectTransform.anchoredPosition = ghostOrigin;
                portraitGhost.enabled = false;
            }

            if (roleText != null)
            {
                roleText.rectTransform.anchoredPosition = roleOrigin;
            }
        }

        private void StopHandles()
        {
            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
