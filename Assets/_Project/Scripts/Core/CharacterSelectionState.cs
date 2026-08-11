using System;
using TW08.Data;
using UnityEngine;

namespace TW08.Core
{
    public static class CharacterSelectionState
    {
        private const string SelectedCharacterKey = "tw08.selected-character";
        private const string DefaultCharacterId = "john";

        public static string SelectedCharacterId
        {
            get
            {
                string value = PlayerPrefs.GetString(SelectedCharacterKey, DefaultCharacterId);
                return string.IsNullOrWhiteSpace(value) ? DefaultCharacterId : value;
            }
        }

        public static void Select(CharacterProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            Select(profile.CharacterId);
        }

        public static void Select(string characterId)
        {
            string normalized = string.IsNullOrWhiteSpace(characterId)
                ? DefaultCharacterId
                : characterId.Trim().ToLowerInvariant();
            PlayerPrefs.SetString(SelectedCharacterKey, normalized);
            PlayerPrefs.Save();
        }
    }
}
