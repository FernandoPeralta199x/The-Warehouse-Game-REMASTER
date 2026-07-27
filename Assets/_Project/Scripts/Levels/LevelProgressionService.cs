using System;
using System.Collections.Generic;

namespace TW08.Levels
{
    public interface ILevelProgressSnapshot
    {
        bool IsCompleted(string levelId);
        int GetMedal(string levelId);
    }

    public static class LevelProgressionService
    {
        public static bool IsUnlocked(LevelDefinition level, ILevelProgressSnapshot progress)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            IReadOnlyList<LevelUnlockRequirement> requirements = level.Requirements;
            for (int i = 0; i < requirements.Count; i++)
            {
                LevelUnlockRequirement requirement = requirements[i];
                if (requirement == null || string.IsNullOrWhiteSpace(requirement.RequiredLevelId))
                {
                    continue;
                }

                if (!progress.IsCompleted(requirement.RequiredLevelId) ||
                    progress.GetMedal(requirement.RequiredLevelId) < requirement.MinimumMedal)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
