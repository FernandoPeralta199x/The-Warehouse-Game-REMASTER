#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    internal static class TW08ProductionExpansionValidator
    {
        internal static IReadOnlyList<string> Validate(
            TW08ExpansionDataSetup.ExpansionData data,
            IReadOnlyCollection<string> menuPaths,
            IReadOnlyCollection<string> puzzlePaths,
            IReadOnlyCollection<string> racePaths)
        {
            List<string> errors = new();
            if (data == null)
            {
                errors.Add("Expansion data is null.");
                return errors;
            }

            if (data.Roster == null || data.Roster.Characters.Count < 3) errors.Add("Character roster must contain John, Duda and Robert.");
            if (data.Roster != null && data.Roster.Find("john") == null) errors.Add("John profile is missing.");
            if (data.Roster != null && data.Roster.Find("duda") == null) errors.Add("Duda profile is missing.");
            if (data.Roster != null && data.Roster.Find("robert") == null) errors.Add("Robert profile is missing.");

            if (data.PuzzleLevels == null)
            {
                errors.Add("Puzzle level collection is null.");
            }
            else
            {
                if (data.PuzzleLevels.Count < 9) errors.Add($"Expected at least the 9 stable puzzle levels, found {data.PuzzleLevels.Count}.");
                foreach (PuzzleLevelDefinition level in data.PuzzleLevels)
                {
                    // UnityEngine.Object can be a live CLR reference while Unity considers the native
                    // object destroyed. Never use the C# null-conditional operator on it.
                    if (level == null)
                    {
                        errors.Add("Puzzle level reference is missing or was invalidated by the AssetDatabase.");
                        continue;
                    }

                    string levelName = string.IsNullOrWhiteSpace(level.LevelId) ? level.name : level.LevelId;
                    IReadOnlyList<string> levelErrors = PuzzleLevelValidator.Validate(level);
                    foreach (string levelError in levelErrors)
                    {
                        errors.Add($"{levelName}: {levelError}");
                    }
                }
            }

            if (data.PuzzleCampaign == null || data.PuzzleCampaign.Levels.Count < 9) errors.Add("Puzzle campaign must expose at least the 9 stable entries.");
            if (data.RaceTracks == null)
            {
                errors.Add("Race track collection is null.");
            }
            else if (data.RaceTracks.Count < 3)
            {
                errors.Add($"Expected at least 3 race tracks, found {data.RaceTracks.Count}.");
            }
            if (data.RaceCampaign == null || data.RaceCampaign.Tracks.Count < 3) errors.Add("Race campaign must expose at least 3 tracks.");
            if (data.ForkliftStats == null) errors.Add("Forklift stats are missing.");

            if (data.RaceTracks != null)
            {
                foreach (var track in data.RaceTracks)
                {
                    if (track == null) errors.Add("Race track entry is null or was invalidated by the AssetDatabase.");
                    else if (track.RaceRules == null) errors.Add($"Race track '{track.DisplayName}' has no RaceDefinition.");
                }
            }

            ValidateScenePaths("menu", menuPaths, errors);
            ValidateScenePaths("puzzle", puzzlePaths, errors);
            ValidateScenePaths("race", racePaths, errors);
            return errors;
        }

        private static void ValidateScenePaths(string category, IEnumerable<string> paths, List<string> errors)
        {
            if (paths == null)
            {
                errors.Add($"Generated {category} scene path collection is null.");
                return;
            }

            foreach (string path in paths)
            {
                if (string.IsNullOrWhiteSpace(path) || AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    errors.Add($"Generated {category} scene is missing: '{path}'.");
                }
            }
        }
    }
}
#endif