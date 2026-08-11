#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TW08.Presentation;
using TW08.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    public static class TW08FullProductionExpansionSetup
    {
        [MenuItem("Tools/TW08/Production/Build Full Production Expansion")]
        public static void BuildFullProductionExpansion()
        {
            try
            {
                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Validando dados base...", 0.05f);
                TW08BasePuzzleDataRecovery.EnsureBaseLevels();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando dados, personagens e campanhas...", 0.16f);
                TW08ExpansionDataSetup.ExpansionData data = TW08ExpansionDataSetup.EnsureAll();
                TW08ArtCatalog catalog = TW08ProductionArtSetup.EnsureProductionArtAssets();
                TW08ExpansionStarterArt.EnsureAll();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando hub e menus...", 0.32f);
                List<string> menuPaths = TW08MenuSceneBuilder.BuildAll(data);
                FixModeSelectNavigation();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando 9 fases de puzzle...", 0.52f);
                List<string> puzzlePaths = TW08PuzzleSceneBuilder.BuildAll(data, catalog);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando 3 pistas de corrida...", 0.74f);
                List<string> racePaths = TW08RaceSceneBuilder.BuildAll(data);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Injetando VFX de gameplay...", 0.84f);
                TW08FeedbackSceneUpgrade.Apply(puzzlePaths, racePaths);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Validando conteúdo e Build Settings...", 0.92f);
                IReadOnlyList<string> validationErrors = TW08ProductionExpansionValidator.Validate(data, menuPaths, puzzlePaths, racePaths);
                if (validationErrors.Count > 0)
                {
                    throw new System.InvalidOperationException(
                        "A expansão gerada falhou na validação estática:\n- " + string.Join("\n- ", validationErrors));
                }

                ConfigureBuildSettings(menuPaths, puzzlePaths, racePaths);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(TW08MenuSceneBuilder.MainMenuPath, OpenSceneMode.Single);

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Produção",
                    "Expansão de produção materializada e validada estruturalmente.\n\n" +
                    "Operadores: John + Duda jogáveis, Robert NPC\n" +
                    "Puzzle: 9 fases\n" +
                    "Corrida: 3 pistas\n" +
                    "Menus: Hub, Operadores, Campanha, Corrida, Configurações e Créditos\n" +
                    "Save: schema v2 + migração v1\n" +
                    "VFX: puzzle + drift/finish\n\n" +
                    "Gate obrigatório restante: Console sem erros + EditMode/PlayMode Test Runner + playtest manual.",
                    "OK");
            }
            catch (System.Exception exception)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Falha no Build",
                    "O gerador interrompeu sem considerar a expansão validada. Veja o primeiro erro no Console.\n\n" + exception.Message,
                    "OK");
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void FixModeSelectNavigation()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TW08MenuSceneBuilder.ModePath) == null) return;
            Scene scene = EditorSceneManager.OpenScene(TW08MenuSceneBuilder.ModePath, OpenSceneMode.Single);
            ModeSelectMenuController controller = Object.FindFirstObjectByType<ModeSelectMenuController>();
            if (controller == null) return;

            BindButton("Settings", controller.OpenSettings);
            BindButton("Credits", controller.OpenCredits);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TW08MenuSceneBuilder.ModePath);
        }

        private static void BindButton(string objectName, UnityEngine.Events.UnityAction action)
        {
            GameObject go = GameObject.Find(objectName);
            Button button = go != null ? go.GetComponent<Button>() : null;
            if (button == null) return;
            while (button.onClick.GetPersistentEventCount() > 0)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, 0);
            }
            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
        }

        private static void ConfigureBuildSettings(
            IEnumerable<string> menuPaths,
            IEnumerable<string> puzzlePaths,
            IEnumerable<string> racePaths)
        {
            List<string> ordered = new();
            AddIfExists(ordered, TW08MenuSceneBuilder.MainMenuPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.ModePath);
            AddIfExists(ordered, TW08MenuSceneBuilder.OperatorPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.PuzzleSelectPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.RaceSelectPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.SettingsPath);
            AddIfExists(ordered, TW08MenuSceneBuilder.CreditsPath);

            foreach (string path in puzzlePaths ?? Enumerable.Empty<string>()) AddIfExists(ordered, path);
            foreach (string path in racePaths ?? Enumerable.Empty<string>()) AddIfExists(ordered, path);

            EditorBuildSettings.scenes = ordered
                .Distinct()
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();
        }

        private static void AddIfExists(ICollection<string> paths, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                paths.Add(path);
            }
        }
    }
}
#endif
