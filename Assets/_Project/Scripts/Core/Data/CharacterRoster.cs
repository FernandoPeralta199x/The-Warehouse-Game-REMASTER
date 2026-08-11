using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Core;
using UnityEngine;

namespace TW08.Data
{
    [CreateAssetMenu(fileName = "CharacterRoster", menuName = "TW08/Characters/Character Roster")]
    public sealed class CharacterRoster : ScriptableObject
    {
        [SerializeField] private string defaultCharacterId = "john";
        [SerializeField] private List<CharacterProfile> characters = new();

        public string DefaultCharacterId => defaultCharacterId;
        public IReadOnlyList<CharacterProfile> Characters => characters;

        public CharacterProfile Find(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            return characters.FirstOrDefault(character =>
                character != null && string.Equals(character.CharacterId, characterId, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<CharacterProfile> GetAvailable(GameMode mode)
        {
            return characters
                .Where(character => character != null)
                .Where(character => mode == GameMode.Race ? character.RaceEnabled : character.PuzzleEnabled)
                .ToArray();
        }

        public CharacterProfile GetDefault()
        {
            return Find(defaultCharacterId) ?? characters.FirstOrDefault(character => character != null);
        }

        private void OnValidate()
        {
            defaultCharacterId = string.IsNullOrWhiteSpace(defaultCharacterId) ? "john" : defaultCharacterId.Trim().ToLowerInvariant();
        }
    }
}
