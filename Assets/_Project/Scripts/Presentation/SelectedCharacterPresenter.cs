using TW08.Core;
using TW08.Data;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class SelectedCharacterPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterRoster roster;
        [SerializeField] private DirectionalSpriteAnimator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public CharacterProfile ActiveProfile { get; private set; }

        public void Configure(CharacterRoster characterRoster, DirectionalSpriteAnimator directionalAnimator, SpriteRenderer renderer)
        {
            roster = characterRoster;
            animator = directionalAnimator;
            spriteRenderer = renderer;
            MarkDirtyInEditor();
        }

        private void Start()
        {
            ApplySelectedCharacter();
        }

        public void ApplySelectedCharacter()
        {
            if (roster == null)
            {
                return;
            }

            CharacterProfile profile = roster.Find(CharacterSelectionState.SelectedCharacterId) ?? roster.GetDefault();
            if (profile == null || !profile.PuzzleEnabled)
            {
                profile = roster.GetDefault();
            }

            ActiveProfile = profile;
            if (profile == null || profile.PuzzleSprites == null)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<DirectionalSpriteAnimator>();
            }

            if (animator != null && spriteRenderer != null)
            {
                animator.Configure(spriteRenderer, profile.PuzzleSprites);
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
