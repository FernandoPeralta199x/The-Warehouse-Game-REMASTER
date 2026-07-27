using System;
using System.Collections.Generic;
using System.Linq;

namespace TW08.Save
{
    [Serializable]
    public sealed class LevelProgressRecord
    {
        public string levelId;
        public int bestMoves;
        public float bestTimeSeconds;
        public int medal;
        public bool completed;
    }

    [Serializable]
    public sealed class SaveGameData
    {
        public int version = 1;
        public string lastUnlockedLevel = "prototype-001";
        public int credits;
        public List<LevelProgressRecord> levels = new();

        public LevelProgressRecord GetOrCreateLevel(string levelId)
        {
            LevelProgressRecord record = levels.FirstOrDefault(item => item.levelId == levelId);
            if (record != null)
            {
                return record;
            }

            record = new LevelProgressRecord { levelId = levelId };
            levels.Add(record);
            return record;
        }
    }
}
