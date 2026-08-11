using System;
using UnityEngine;

namespace TW08.Puzzle
{
    public static class PuzzleProgressStore
    {
        private const string Prefix = "tw08.puzzle.";

        public static void RecordCompletion(PuzzleLevelDefinition level, int moves)
        {
            if (level == null || string.IsNullOrWhiteSpace(level.LevelId))
            {
                return;
            }

            string id = Normalize(level.LevelId);
            PlayerPrefs.SetInt(Prefix + id + ".completed", 1);

            int best = PlayerPrefs.GetInt(Prefix + id + ".bestMoves", 0);
            if (best <= 0 || moves < best)
            {
                PlayerPrefs.SetInt(Prefix + id + ".bestMoves", Mathf.Max(0, moves));
            }

            int medal = EvaluateMedal(level, moves);
            int previousMedal = PlayerPrefs.GetInt(Prefix + id + ".medal", 0);
            if (medal > previousMedal)
            {
                PlayerPrefs.SetInt(Prefix + id + ".medal", medal);
            }

            PlayerPrefs.Save();
        }

        public static bool IsCompleted(string levelId)
        {
            return PlayerPrefs.GetInt(Prefix + Normalize(levelId) + ".completed", 0) == 1;
        }

        public static int GetBestMoves(string levelId)
        {
            return PlayerPrefs.GetInt(Prefix + Normalize(levelId) + ".bestMoves", 0);
        }

        public static int GetMedal(string levelId)
        {
            return PlayerPrefs.GetInt(Prefix + Normalize(levelId) + ".medal", 0);
        }

        public static bool IsUnlocked(PuzzleCampaignDefinition campaign, int index)
        {
            if (campaign == null || index < 0 || index >= campaign.Levels.Count)
            {
                return false;
            }

            PuzzleCampaignEntry entry = campaign.Levels[index];
            if (index == 0 || (entry != null && entry.UnlockedByDefault))
            {
                return true;
            }

            PuzzleCampaignEntry previous = campaign.Levels[index - 1];
            return previous != null && previous.Level != null && IsCompleted(previous.Level.LevelId);
        }

        public static int EvaluateMedal(PuzzleLevelDefinition level, int moves)
        {
            if (level == null)
            {
                return 0;
            }

            if (level.PlatinumMoveLimit > 0 && moves <= level.PlatinumMoveLimit)
            {
                return 3;
            }

            if (level.GoldMoveLimit > 0 && moves <= level.GoldMoveLimit)
            {
                return 2;
            }

            return 1;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
        }
    }
}
