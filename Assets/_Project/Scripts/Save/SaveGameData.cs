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
    public sealed class RaceProgressRecord
    {
        public string trackId;
        public float bestTimeSeconds;
        public int medal;
        public bool completed;
    }

    [Serializable]
    public sealed class SaveGameData
    {
        public int version = 2;
        public string selectedCharacterId = "john";
        public string lastUnlockedLevel = "tw08-s01-001";
        public int credits;
        public List<string> unlockedCharacters = new() { "john", "duda" };
        public List<LevelProgressRecord> levels = new();
        public List<RaceProgressRecord> races = new();
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;

        public LevelProgressRecord GetOrCreateLevel(string levelId)
        {
            levels ??= new List<LevelProgressRecord>();
            LevelProgressRecord record = levels.FirstOrDefault(item => item != null && item.levelId == levelId);
            if (record != null)
            {
                return record;
            }

            record = new LevelProgressRecord { levelId = levelId };
            levels.Add(record);
            return record;
        }

        public RaceProgressRecord GetOrCreateRace(string trackId)
        {
            races ??= new List<RaceProgressRecord>();
            RaceProgressRecord record = races.FirstOrDefault(item => item != null && item.trackId == trackId);
            if (record != null)
            {
                return record;
            }

            record = new RaceProgressRecord { trackId = trackId };
            races.Add(record);
            return record;
        }

        public void EnsureDefaults()
        {
            selectedCharacterId = string.IsNullOrWhiteSpace(selectedCharacterId) ? "john" : selectedCharacterId;
            lastUnlockedLevel = string.IsNullOrWhiteSpace(lastUnlockedLevel) ? "tw08-s01-001" : lastUnlockedLevel;
            levels ??= new List<LevelProgressRecord>();
            races ??= new List<RaceProgressRecord>();
            unlockedCharacters ??= new List<string>();
            if (!unlockedCharacters.Contains("john")) unlockedCharacters.Add("john");
            if (!unlockedCharacters.Contains("duda")) unlockedCharacters.Add("duda");
            masterVolume = Clamp01(masterVolume <= 0f ? 1f : masterVolume);
            musicVolume = Clamp01(musicVolume <= 0f ? 0.8f : musicVolume);
            sfxVolume = Clamp01(sfxVolume <= 0f ? 1f : sfxVolume);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
