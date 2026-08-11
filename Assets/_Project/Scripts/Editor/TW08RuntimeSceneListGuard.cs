#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace TW08.Editor
{
    [InitializeOnLoad]
    internal static class TW08RuntimeSceneListGuard
    {
        private static bool applying;
        private static bool scheduled;

        static TW08RuntimeSceneListGuard()
        {
            EditorBuildSettings.sceneListChanged += ScheduleSynchronization;
            ScheduleSynchronization();
        }

        [MenuItem("Tools/TW08/Production/Synchronize Runtime Scene List")]
        internal static void SynchronizeNow()
        {
            Synchronize(showDialog: true);
        }

        private static void ScheduleSynchronization()
        {
            if (applying || scheduled || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            scheduled = true;
            EditorApplication.delayCall += RunScheduledSynchronization;
        }

        private static void RunScheduledSynchronization()
        {
            scheduled = false;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Synchronize(showDialog: false);
            }
        }

        private static void Synchronize(bool showDialog)
        {
            if (applying)
            {
                return;
            }

            applying = true;
            try
            {
                List<string> scenePaths = DiscoverRuntimeScenes();
                if (scenePaths.Count == 0)
                {
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog(
                            "The Warehouse Nº 08 — Scene List",
                            "Nenhuma cena TW08 de runtime foi encontrada. Execute primeiro Build Full Production Expansion.",
                            "OK");
                    }
                    return;
                }

                EditorBuildSettingsScene[] entries = scenePaths
                    .Select(path => new EditorBuildSettingsScene(path, true))
                    .ToArray();

                EditorBuildSettings.globalScenes = entries;

                BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
                if (activeProfile != null && activeProfile.overrideGlobalScenes)
                {
                    activeProfile.overrideGlobalScenes = false;
                    EditorUtility.SetDirty(activeProfile);
                }

                // With overrideGlobalScenes disabled this resolves to the global list. Setting it as
                // well makes the current Editor session observe the synchronized list immediately.
                EditorBuildSettings.scenes = entries;
                AssetDatabase.SaveAssets();

                Validate(entries);

                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        "The Warehouse Nº 08 — Scene List",
                        $"Scene List sincronizada com sucesso.\n\nCenas registradas: {entries.Length}\n" +
                        "Fonte: lista global compartilhada\nBuild Profile ativo: usando lista global",
                        "OK");
                }
            }
            finally
            {
                applying = false;
            }
        }

        private static List<string> DiscoverRuntimeScenes()
        {
            List<string> ordered = new();
            AddIfExists(ordered, TW08MenuSceneBuilder.MainMenuPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.ModePath);
            AddIfExists(ordered, TW08MenuSceneBuilder.OperatorPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.PuzzleSelectPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.RaceSelectPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.SettingsPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.CreditsPath);

            foreach (string path in FindScenes(TW08PuzzleSceneBuilder.SceneRoot, "TW08_Level"))
            {
                AddIfExists(ordered, path);
            }

            foreach (string path in FindScenes(TW08RaceSceneBuilder.SceneRoot, "TW08_Race"))
            {
                AddIfExists(ordered, path);
            }

            return ordered.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static IEnumerable<string> FindScenes(string root, string filePrefix)
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    !string.IsNullOrWhiteSpace(path) &&
                    Path.GetFileNameWithoutExtension(path).StartsWith(filePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        private static void AddIfExists(ICollection<string> paths, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                paths.Add(path);
            }
        }

        private static void Validate(IEnumerable<EditorBuildSettingsScene> expected)
        {
            HashSet<string> global = new(
                EditorBuildSettings.globalScenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path),
                StringComparer.OrdinalIgnoreCase);
            HashSet<string> active = new(
                EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path),
                StringComparer.OrdinalIgnoreCase);

            List<string> missing = expected
                .Where(scene => scene != null && scene.enabled)
                .Select(scene => scene.path)
                .Where(path => !global.Contains(path) || !active.Contains(path))
                .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "TW08 Scene List synchronization failed. Missing from global or active list:\n- " +
                    string.Join("\n- ", missing));
            }
        }
    }
}
#endif
