using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "TW08/Levels/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private List<LevelDefinition> levels = new();

        public IReadOnlyList<LevelDefinition> Levels => levels;

        public bool TryGet(string levelId, out LevelDefinition level)
        {
            level = levels.FirstOrDefault(item => item != null && item.LevelId == levelId);
            return level != null;
        }

        public IReadOnlyList<string> ValidateCatalog()
        {
            List<string> errors = new();
            HashSet<string> ids = new(StringComparer.Ordinal);

            foreach (LevelDefinition level in levels)
            {
                if (level == null)
                {
                    errors.Add("Catalog contains a null level reference.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(level.LevelId))
                {
                    errors.Add($"Level '{level.name}' has an empty id.");
                    continue;
                }

                if (!ids.Add(level.LevelId))
                {
                    errors.Add($"Duplicate level id: {level.LevelId}.");
                }
            }

            return errors;
        }
    }
}
