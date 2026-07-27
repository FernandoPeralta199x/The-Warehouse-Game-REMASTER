#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TW08.Levels;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    public static class TW08ProjectValidator
    {
        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project/Art",
            "Assets/_Project/Audio",
            "Assets/_Project/Prefabs",
            "Assets/_Project/Scenes",
            "Assets/_Project/Scripts",
            "Assets/_Project/ScriptableObjects",
            "Assets/_Project/Settings",
            "Assets/_Project/Tests"
        };

        [MenuItem("Tools/TW08/Validate Project")]
        public static void ValidateProject()
        {
            List<string> errors = new();
            List<string> warnings = new();

            foreach (string folder in RequiredFolders)
            {
                if (!Directory.Exists(folder))
                {
                    errors.Add($"Missing required folder: {folder}");
                }
            }

            string[] levelGuids = AssetDatabase.FindAssets("t:PuzzleLevelDefinition");
            foreach (string guid in levelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PuzzleLevelDefinition level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(path);
                IReadOnlyList<string> levelErrors = PuzzleLevelValidator.Validate(level);
                errors.AddRange(levelErrors.Select(error => $"{path}: {error}"));
            }

            string[] catalogGuids = AssetDatabase.FindAssets("t:LevelCatalog");
            foreach (string guid in catalogGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(path);
                errors.AddRange(catalog.ValidateCatalog().Select(error => $"{path}: {error}"));
            }

            if (AssetDatabase.FindAssets("t:AudioMixer").Length == 0)
            {
                warnings.Add("No AudioMixer asset exists yet. This is acceptable before audio integration.");
            }

            string message = $"Errors: {errors.Count}\nWarnings: {warnings.Count}";
            if (errors.Count > 0)
            {
                Debug.LogError(message + "\n- " + string.Join("\n- ", errors));
                EditorUtility.DisplayDialog("TW08 Validation", message + "\nSee Console for details.", "OK");
                return;
            }

            if (warnings.Count > 0)
            {
                Debug.LogWarning(message + "\n- " + string.Join("\n- ", warnings));
            }

            Debug.Log("TW08 project validation completed without blocking errors.");
            EditorUtility.DisplayDialog("TW08 Validation", message, "OK");
        }
    }
}
#endif
