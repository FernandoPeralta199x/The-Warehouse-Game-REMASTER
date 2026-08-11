#if UNITY_EDITOR
using System;
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
            PuzzleRuntime runtime = UnityEngine.Object.FindFirstObjectByType<PuzzleRuntime>();
            if (runtime == null) return;

            PuzzleVfxFeedback vfx = runtime.GetComponent<PuzzleVfxFeedback>();
            if (vfx == null)
            {
                vfx = runtime.gameObject.AddComponent<PuzzleVfxFeedback>();
            }
            if (vfx == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach PuzzleVfxFeedback in '{path}'.");
            }

            vfx.Configure(runtime);
            EditorUtility.SetDirty(vfx);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save puzzle VFX upgrade for '{path}'.");
            }
        }

        private static void UpgradeRace(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ArcadeForkliftController2D vehicle = UnityEngine.Object.FindFirstObjectByType<ArcadeForkliftController2D>();
            RaceSessionController session = UnityEngine.Object.FindFirstObjectByType<RaceSessionController>();
            if (vehicle == null || session == null) return;

            ForkliftVfxController vfx = vehicle.GetComponent<ForkliftVfxController>();
            if (vfx == null)
            {
                vfx = vehicle.gameObject.AddComponent<ForkliftVfxController>();
            }
            if (vfx == null)
            {
                throw new InvalidOperationException($"TW08 failed to attach ForkliftVfxController in '{path}'.");
            }

            vfx.Configure(vehicle, session);
            EditorUtility.SetDirty(vfx);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save race VFX upgrade for '{path}'.");
            }
        }
    }
}
#endif