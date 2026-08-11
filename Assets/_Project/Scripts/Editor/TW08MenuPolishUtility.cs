#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TW08.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    internal static class TW08MenuPolishUtility
    {
        internal static void Apply()
        {
            foreach (string path in GetMenuPaths())
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                GameObject shell = GameObject.Find("Terminal Shell");
                if (canvas == null || shell == null)
                {
                    Debug.LogWarning($"TW08 menu polish skipped '{path}' because Canvas/Terminal Shell was not found.");
                    continue;
                }

                TerminalGridGraphic grid = FindGrid(canvas.transform);
                if (grid == null)
                {
                    GameObject go = new(
                        "Terminal Grid Overlay",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(TerminalGridGraphic));
                    go.transform.SetParent(canvas.transform, false);
                    RectTransform rect = go.GetComponent<RectTransform>();
                    TW08ProductionSceneUtility.Stretch(rect);
                    go.transform.SetSiblingIndex(Mathf.Max(0, shell.transform.GetSiblingIndex()));
                    grid = go.GetComponent<TerminalGridGraphic>();
                }

                grid.Configure(new Color(0.20f, 0.90f, 0.47f, 0.055f), 72f, 9f, 1f);
                grid.raycastTarget = false;
                EditorUtility.SetDirty(grid);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new InvalidOperationException($"TW08 failed to save menu polish for '{path}'.");
                }
            }
        }

        private static TerminalGridGraphic FindGrid(Transform canvas)
        {
            foreach (TerminalGridGraphic grid in canvas.GetComponentsInChildren<TerminalGridGraphic>(true))
            {
                if (grid != null && string.Equals(grid.gameObject.name, "Terminal Grid Overlay", StringComparison.Ordinal))
                {
                    return grid;
                }
            }
            return null;
        }

        private static IEnumerable<string> GetMenuPaths()
        {
            yield return TW08MenuSceneBuilder.MainMenuPath;
            yield return TW08MenuSceneBuilder.ModePath;
            yield return TW08MenuSceneBuilder.OperatorPath;
            yield return TW08MenuSceneBuilder.PuzzleSelectPath;
            yield return TW08MenuSceneBuilder.RaceSelectPath;
            yield return TW08MenuSceneBuilder.SettingsPath;
            yield return TW08MenuSceneBuilder.CreditsPath;
        }
    }
}
#endif
