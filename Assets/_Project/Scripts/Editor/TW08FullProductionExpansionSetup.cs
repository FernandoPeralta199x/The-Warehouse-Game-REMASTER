#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TW08.Audio;
using TW08.Data;
using TW08.Presentation;
using TW08.Puzzle;
using TW08.Race;
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

                // Any pipeline that writes/imports PNG/WAV files may call AssetDatabase.Refresh(),
                // which can replace native UnityEngine.Object instances held by local variables.
                // Finish all refresh-heavy generation before capturing campaign references.
                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Gerando arte e áudio starter...", 0.12f);
                TW08ProductionArtSetup.EnsureProductionArtAssets();
                TW08ExpansionStarterArt.EnsureAll();
                TW08StarterAudioSetup.EnsureAll();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando dados, personagens e campanhas...", 0.20f);
                TW08ExpansionDataSetup.EnsureAll();
                AssetDatabase.SaveAssets();

                // EnsureAll and the art pipeline may refresh the AssetDatabase. Never continue with
                // references captured before those refreshes; reload the serialized assets by path.
                TW08ExpansionDataSetup.ExpansionData data = ReloadStableExpansionData();
                TW08ArtCatalog catalog = RequireAsset<TW08ArtCatalog>(TW08ProductionArtSetup.CatalogPath);
                TW08AudioCatalog audioCatalog = RequireAsset<TW08AudioCatalog>(TW08StarterAudioSetup.CatalogPath);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando hub e menus...", 0.32f);
                List<string> menuPaths = TW08MenuSceneBuilder.BuildAll(data);
                FixModeSelectNavigation();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando 9 fases de puzzle...", 0.52f);
                List<string> puzzlePaths = TW08PuzzleSceneBuilder.BuildAll(data, catalog);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Criando 3 pistas de corrida...", 0.72f);
                List<string> racePaths = TW08RaceSceneBuilder.BuildAll(data);

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Injetando VFX e áudio...", 0.82f);
                TW08FeedbackSceneUpgrade.Apply(puzzlePaths, racePaths);
                TW08AudioSceneUpgrade.Apply(audioCatalog, menuPaths, puzzlePaths, racePaths);

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
                    "VFX: puzzle + drift/finish\n" +
                    "Áudio: SFX + loops starter para menu, puzzle e corrida\n\n" +
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

        private static TW08ExpansionDataSetup.ExpansionData ReloadStableExpansionData()
        {
            CharacterRoster roster = RequireAsset<CharacterRoster>(TW08ExpansionDataSetup.RosterPath);
            PuzzleCampaignDefinition puzzleCampaign = RequireAsset<PuzzleCampaignDefinition>(TW08ExpansionDataSetup.PuzzleCampaignPath);
            RaceCampaignDefinition raceCampaign = RequireAsset<RaceCampaignDefinition>(TW08ExpansionDataSetup.RaceCampaignPath);
            ForkliftStats forkliftStats = RequireAsset<ForkliftStats>(TW08ExpansionDataSetup.ForkliftStatsPath);

            List<PuzzleLevelDefinition> puzzleLevels = puzzleCampaign.Levels
                .Where(entry => entry != null && entry.Level != null)
                .Select(entry => entry.Level)
                .ToList();
            List<RaceTrackDefinition> raceTracks = raceCampaign.Tracks
                .Where(track => track != null)
                .ToList();

            if (puzzleLevels.Count != 9)
            {
                throw new System.InvalidOperationException(
                    $"Campanha puzzle deveria conter 9 fases válidas após recarregar o AssetDatabase, mas contém {puzzleLevels.Count}.");
            }

            if (raceTracks.Count != 3)
            {
                throw new System.InvalidOperationException(
                    $"Campanha de corrida deveria conter 3 pistas válidas após recarregar o AssetDatabase, mas contém {raceTracks.Count}.");
            }

            return new TW08ExpansionDataSetup.ExpansionData
            {
                Roster = roster,
                PuzzleCampaign = puzzleCampaign,
                RaceCampaign = raceCampaign,
                ForkliftStats = forkliftStats,
                PuzzleLevels = puzzleLevels,
                RaceTracks = raceTracks
            };
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new System.InvalidOperationException(
                    $"Asset obrigatório não pôde ser recarregado após o refresh: {path} ({typeof(T).Name}).");
            }

            return asset;
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
