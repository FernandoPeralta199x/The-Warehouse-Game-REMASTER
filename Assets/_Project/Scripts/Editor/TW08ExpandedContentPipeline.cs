#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    /// <summary>
    /// Pipeline batchmode de expansão de conteúdo:
    /// 1. Importa fases do JSON enriquecido (assets + campanhas).
    /// 2. Reconstrói produção completa (menus, cenas de puzzle, corridas).
    /// 3. Constrói cenas da campanha secreta.
    /// 4. Sincroniza a Scene List global incluindo as secretas.
    ///
    /// Uso: Unity -batchmode -executeMethod TW08.Editor.TW08ExpandedContentPipeline.RunFromCommandLine
    /// </summary>
    public static class TW08ExpandedContentPipeline
    {
        [MenuItem("Tools/TW08/Production/Run Expanded Content Pipeline")]
        public static void RunMenu()
        {
            Run();
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08",
                "Pipeline de conteúdo expandido concluído.",
                "OK");
        }

        public static void RunFromCommandLine()
        {
            try
            {
                Run();
                Debug.Log("TW08ExpandedContentPipeline: concluído com sucesso.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"TW08ExpandedContentPipeline FALHOU: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static void Run()
        {
            // 1. Fases novas (assets + registro nas campanhas).
            int imported = TW08CampaignExpansionImporter.ImportAll();
            Debug.Log($"Pipeline: {imported} fases importadas.");

            // 2. Produção completa — menus (com grade rolável + arquivo secreto),
            //    todas as cenas de puzzle da campanha principal e corridas.
            TW08FullProductionExpansionSetup.BuildFullProductionExpansion();

            // 3. Cenas das fases secretas.
            var secretCampaign = AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(
                TW08CampaignExpansionImporter.SecretCampaignPath);
            List<string> secretScenes = new();
            if (secretCampaign != null && secretCampaign.Levels.Count > 0)
            {
                secretScenes = TW08PuzzleSceneBuilder.BuildForCampaign(
                    secretCampaign,
                    TW08PuzzleSceneBuilder.SecretSceneRoot,
                    "TW08_SecretSelect");
                Debug.Log($"Pipeline: {secretScenes.Count} cenas secretas construídas.");
            }

            // 4. Scene List global: base (já configurada pelo passo 2) + secretas.
            if (secretScenes.Count > 0)
            {
                var existing = EditorBuildSettings.globalScenes?.ToList()
                               ?? new List<EditorBuildSettingsScene>();
                var known = new HashSet<string>(
                    existing.Select(scene => scene.path),
                    StringComparer.OrdinalIgnoreCase);

                if (System.IO.File.Exists(TW08MenuSceneBuilder.SecretSelectPath)
                    && !known.Contains(TW08MenuSceneBuilder.SecretSelectPath))
                {
                    existing.Add(new EditorBuildSettingsScene(TW08MenuSceneBuilder.SecretSelectPath, true));
                    known.Add(TW08MenuSceneBuilder.SecretSelectPath);
                }

                foreach (string path in secretScenes.Where(path => !known.Contains(path)))
                {
                    existing.Add(new EditorBuildSettingsScene(path, true));
                    known.Add(path);
                }

                EditorBuildSettings.globalScenes = existing.ToArray();
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Pipeline: Scene List com {EditorBuildSettings.globalScenes.Length} cenas.");
        }
    }
}
#endif
