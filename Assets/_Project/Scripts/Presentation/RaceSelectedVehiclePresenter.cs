using TW08.Core;
using UnityEngine;

namespace TW08.Presentation
{
    [DisallowMultipleComponent]
    public sealed class RaceSelectedVehiclePresenter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite johnForklift;
        [SerializeField] private Sprite dudaForklift;

        public void Configure(SpriteRenderer renderer, Sprite john, Sprite duda)
        {
            spriteRenderer = renderer;
            johnForklift = john;
            dudaForklift = duda;
            MarkDirtyInEditor();
        }

        private void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = CharacterSelectionState.SelectedCharacterId == "duda" && dudaForklift != null
                ? dudaForklift
                : johnForklift;
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
