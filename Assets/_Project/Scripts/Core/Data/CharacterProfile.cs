using TW08.Presentation;
using UnityEngine;

namespace TW08.Data
{
    [CreateAssetMenu(fileName = "CharacterProfile", menuName = "TW08/Characters/Character Profile")]
    public sealed class CharacterProfile : ScriptableObject
    {
        [SerializeField] private string characterId = "john";
        [SerializeField] private string displayName = "John Miller";
        [SerializeField] private string role = "Operador";
        [SerializeField, TextArea(2, 5)] private string description = string.Empty;
        [SerializeField] private Sprite portrait;
        [SerializeField] private DirectionalSpriteSet puzzleSprites;
        [SerializeField] private Color uiAccent = new(1f, 0.65f, 0.12f, 1f);
        [SerializeField] private bool puzzleEnabled = true;
        [SerializeField] private bool raceEnabled = true;
        [SerializeField] private bool unlockedByDefault = true;

        public string CharacterId => string.IsNullOrWhiteSpace(characterId) ? name : characterId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Role => role;
        public string Description => description;
        public Sprite Portrait => portrait;
        public DirectionalSpriteSet PuzzleSprites => puzzleSprites;
        public Color UiAccent => uiAccent;
        public bool PuzzleEnabled => puzzleEnabled;
        public bool RaceEnabled => raceEnabled;
        public bool UnlockedByDefault => unlockedByDefault;

        private void OnValidate()
        {
            characterId = string.IsNullOrWhiteSpace(characterId) ? name.ToLowerInvariant() : characterId.Trim().ToLowerInvariant();
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
        }
    }
}
