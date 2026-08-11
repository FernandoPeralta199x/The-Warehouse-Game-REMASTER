#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Build do player Windows a partir da Scene List global.
    /// Uso batchmode:
    ///   Unity -batchmode -executeMethod TW08.Editor.TW08BuildPipeline.BuildWindowsFromCommandLine
    /// </summary>
    public static class TW08BuildPipeline
    {
        private const string OutputDir = "Builds/Windows";
        private const string ExecutableName = "TheWarehouseN08.exe";

        [MenuItem("Tools/TW08/Production/Build Windows Player")]
        public static void BuildWindowsMenu()
        {
            BuildReport report = BuildWindows();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08",
                $"Build: {report.summary.result}\n" +
                $"Cenas: {report.summary.totalSize / (1024 * 1024)} MB\n" +
                $"Saída: {OutputDir}/{ExecutableName}",
                "OK");
        }

        public static void BuildWindowsFromCommandLine()
        {
            try
            {
                BuildReport report = BuildWindows();
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"TW08BuildPipeline: build terminou com {report.summary.result} " +
                                   $"({report.summary.totalErrors} erros).");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"TW08BuildPipeline: build OK — {report.summary.outputPath} " +
                          $"({report.summary.totalSize / (1024 * 1024)} MB).");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"TW08BuildPipeline FALHOU: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static BuildReport BuildWindows()
        {
            string[] scenes = EditorBuildSettings.globalScenes
                .Where(scene => scene != null && scene.enabled && File.Exists(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException(
                    "Scene List global vazia — rode o pipeline de conteúdo antes do build.");
            }

            Directory.CreateDirectory(OutputDir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(OutputDir, ExecutableName),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };

            Debug.Log($"TW08BuildPipeline: iniciando build com {scenes.Length} cenas.");
            return BuildPipeline.BuildPlayer(options);
        }
    }
}
#endif
