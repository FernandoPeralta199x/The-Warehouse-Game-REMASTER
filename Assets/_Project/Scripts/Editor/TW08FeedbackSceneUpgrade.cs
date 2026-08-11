#if UNITY_EDITOR
using System.Collections.Generic;
using TW08.Presentation;
using TW08.Puzzle;
using TW08.Race;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Editor
{
    internal static class TW08FeedbackSceneUpgrade
    {
        internal static void Apply(IEnumerable<string> puzzlePaths, IEnumerable<string> racePaths)
        {
            if (puzzlePaths != null)
            {
                foreach (string path in puzzlePaths) UpgradePuzzle(path);
            }

            if (racePaths != null)
            {
                foreach (string path in racePaths) UpgradeRace(path);
            }
        }

        private static void UpgradePuzzle(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PuzzleRuntime runtime = Object.FindFirstObjectByType<PuzzleRuntime>();
            if (runtime == null) return;
            PuzzleVfxFeedback vfx = runtime.GetComponent<PuzzleVfxFeedback>() ?? runtime.gameObject.AddComponent<PuzzleVfxFeedback>();
            vfx.Configure(runtime);
            EditorUtility.SetDirty(vfx);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void UpgradeRace(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ArcadeForkliftController2D vehicle = Object.FindFirstObjectByType<ArcadeForkliftController2D>();
            RaceSessionController session = Object.FindFirstObjectByType<RaceSessionController>();
            if (vehicle == null || session == null) return;
            ForkliftVfxController vfx = vehicle.GetComponent<ForkliftVfxController>() ?? vehicle.gameObject.AddComponent<ForkliftVfxController>();
            vfx.Configure(vehicle, session);
            EditorUtility.SetDirty(vfx);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
#endif
