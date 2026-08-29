#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TW08.Puzzle;
using TW08.Race;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace TW08.Editor
{
    [InitializeOnLoad]
    internal static class TW08RuntimeSceneListGuard
    {
        // Contagem de cenas é derivada dinamicamente das campanhas (puzzle,
        // secreta e corrida); nenhuma constante fixa — a lista cresce com o conteúdo.
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
                List<string> expected = BuildExpectedScenePaths();
                if (expected.Count == 0)
                {
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog(
                            "The Warehouse Nº 08 — Scene List",
                            "Os dados de produção ainda não existem. Execute primeiro Build Full Production Expansion.",
                            "OK");
                    }
                    return;
                }

                List<string> missing = expected.Where(path => !SceneFileExists(path)).ToList();
                if (missing.Count > 0)
                {
                    string details = missing.Count > 0
                        ? "\n\nArquivos de cena ausentes:\n- " + string.Join("\n- ", missing)
                        : string.Empty;
                    string message =
                        $"TW08 Scene List encontrou {expected.Count - missing.Count}/{expected.Count} cenas físicas válidas. " +
                        "A Scene List atual foi preservada." + details +
                        "\n\nExecute Tools > TW08 > Production > Repair Runtime Scene Registration.";
                    Debug.LogWarning(message);
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog("The Warehouse Nº 08 — Scene List", message, "OK");
                    }
                    return;
                }

                EditorBuildSettingsScene[] entries = expected
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(path => new EditorBuildSettingsScene(path, true))
                    .ToArray();

                // Global scenes are the single source of truth. Unity 6 build profiles use this list
                // whenever overrideGlobalScenes is false.
                EditorBuildSettings.globalScenes = entries;

                BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
                if (activeProfile != null && activeProfile.overrideGlobalScenes)
                {
                    activeProfile.overrideGlobalScenes = false;
                    EditorUtility.SetDirty(activeProfile);
                }

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

        private static List<string> BuildExpectedScenePaths()
        {
            PuzzleCampaignDefinition puzzleCampaign =
                AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(TW08ExpansionDataSetup.PuzzleCampaignPath);
            RaceCampaignDefinition raceCampaign =
                AssetDatabase.LoadAssetAtPath<RaceCampaignDefinition>(TW08ExpansionDataSetup.RaceCampaignPath);

            if (puzzleCampaign == null || raceCampaign == null)
            {
                return new List<string>();
            }

            List<string> paths = new()
            {
                TW08MenuSceneBuilder.MainMenuPath,
                TW08MenuSceneBuilder.ModePath,
                TW08MenuSceneBuilder.OperatorPath,
                TW08MenuSceneBuilder.PuzzleSelectPath,
                TW08MenuSceneBuilder.RaceSelectPath,
                TW08MenuSceneBuilder.SettingsPath,
                TW08MenuSceneBuilder.CreditsPath
            };

            if (SceneFileExists(TW08MenuSceneBuilder.SecretSelectPath))
            {
                paths.Add(TW08MenuSceneBuilder.SecretSelectPath);
            }

            if (SceneFileExists(TW08ShopSetup.ShopScenePath))
            {
                paths.Add(TW08ShopSetup.ShopScenePath);
            }

            foreach (PuzzleCampaignEntry entry in puzzleCampaign.Levels)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.SceneName)) continue;
                paths.Add(TW08PuzzleSceneBuilder.SceneRoot + "/" + entry.SceneName + ".unity");
            }

            PuzzleCampaignDefinition secretCampaign =
                AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                    TW08CampaignExpansionImporter.SecretCampaignPath);
            if (secretCampaign != null)
            {
                foreach (PuzzleCampaignEntry entry in secretCampaign.Levels)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.SceneName)) continue;
                    paths.Add(TW08PuzzleSceneBuilder.SecretSceneRoot + "/" + entry.SceneName + ".unity");
                }
            }

            foreach (RaceTrackDefinition track in raceCampaign.Tracks)
            {
                if (track == null || string.IsNullOrWhiteSpace(track.SceneName)) continue;
                paths.Add(TW08RaceSceneBuilder.SceneRoot + "/" + track.SceneName + ".unity");
            }

            return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool SceneFileExists(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath)) return false;
            string fullPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath);
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