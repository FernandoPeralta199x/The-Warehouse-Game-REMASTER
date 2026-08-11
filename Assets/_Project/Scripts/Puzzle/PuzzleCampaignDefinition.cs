using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Puzzle
{
    [Serializable]
    public sealed class PuzzleCampaignEntry
    {
        [SerializeField] private PuzzleLevelDefinition level;
        [SerializeField] private string sceneName;
        [SerializeField] private bool unlockedByDefault;

        public PuzzleLevelDefinition Level => level;
        public string SceneName => sceneName;
        public bool UnlockedByDefault => unlockedByDefault;
    }

    [CreateAssetMenu(fileName = "PuzzleCampaign", menuName = "TW08/Puzzle/Campaign Definition")]
    public sealed class PuzzleCampaignDefinition : ScriptableObject
    {
        [SerializeField] private List<PuzzleCampaignEntry> levels = new();

        public IReadOnlyList<PuzzleCampaignEntry> Levels => levels;

        public PuzzleCampaignEntry Find(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                return null;
            }

            return levels.FirstOrDefault(entry =>
                entry != null && entry.Level != null &&
                string.Equals(entry.Level.LevelId, levelId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
