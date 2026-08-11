#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Audio;
using TW08.Data;
using TW08.Presentation;
using TW08.Puzzle;
using TW08.Race;
using TW08.UI;
using UnityEditor;
using UnityEditor.Build.Profile;
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
                EnsureAudioListeners(menuPaths.Concat(puzzlePaths).Concat(racePaths));

                // Scene authoring and import callbacks can still cause Unity to recreate native asset
                // handles. Validate against freshly loaded campaign assets, never stale local handles.
                data = ReloadStableExpansionData();

                EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Validando conteúdo e Build Settings...", 0.92f);
                IReadOnlyList<string> validationErrors = TW08ProductionExpansionValidator.Validate(data, menuPaths, puzzlePaths, racePaths);
                if (validationErrors.Count > 0)
                {
                    throw new InvalidOperationException(
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
                    "Áudio: SFX + loops starter para menu, puzzle e corrida\n" +
                    "Scene List: compartilhada + perfil ativo sincronizados\n\n" +
                    "Gate obrigatório restante: Console sem erros + EditMode/PlayMode Test Runner + playtest manual.",
                    "OK");
            }
            catch (Exception exception)
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

        [MenuItem("Tools/TW08/Production/Repair Runtime Scene Registration")]
        public static void RepairRuntimeSceneRegistration()
        {
            try
            {
                if (Application.isPlaying)
                {
                    throw new InvalidOperationException("Saia do Play Mode antes de reparar o registro das cenas.");
                }

                TW08ExpansionDataSetup.ExpansionData data = ReloadStableExpansionData();
                List<string> menuPaths = GetMenuPaths();
                List<string> puzzlePaths = GetPuzzlePaths(data);
                List<string> racePaths = GetRacePaths(data);

                ConfigureBuildSettings(menuPaths, puzzlePaths, racePaths);
                EnsureAudioListeners(menuPaths.Concat(puzzlePaths).Concat(racePaths));
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Runtime Repair",
                    "Registro de cenas reparado.\n\n" +
                    "- Scene List compartilhada sincronizada\n" +
                    "- Build Profile ativo sincronizado quando aplicável\n" +
                    "- AudioListener garantido nas cenas geradas\n\n" +
                    "Agora execute o Play Mode e tente abrir a Fase 01 novamente.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "The Warehouse Nº 08 — Falha no Repair",
                    exception.Message,
                    "OK");
                throw;
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
                throw new InvalidOperationException(
                    $"Campanha puzzle deveria conter 9 fases válidas após recarregar o AssetDatabase, mas contém {puzzleLevels.Count}.");
            }

            if (raceTracks.Count != 3)
            {
                throw new InvalidOperationException(
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
                throw new InvalidOperationException(
                    $"Asset obrigatório não pôde ser recarregado após o refresh: {path} ({typeof(T).Name}).");
            }

            return asset;
        }

        private static void FixModeSelectNavigation()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TW08MenuSceneBuilder.ModePath) == null) return;
            Scene scene = EditorSceneManager.OpenScene(TW08MenuSceneBuilder.ModePath, OpenSceneMode.Single);
            ModeSelectMenuController controller = UnityEngine.Object.FindFirstObjectByType<ModeSelectMenuController>();
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

        private static List<string> GetMenuPaths()
        {
            return new List<string>
            {
                TW08MenuSceneBuilder.MainMenuPath,
                TW08MenuSceneBuilder.ModePath,
                TW08MenuSceneBuilder.OperatorPath,
                TW08MenuSceneBuilder.PuzzleSelectPath,
                TW08MenuSceneBuilder.RaceSelectPath,
                TW08MenuSceneBuilder.SettingsPath,
                TW08MenuSceneBuilder.CreditsPath
            };
        }

        private static List<string> GetPuzzlePaths(TW08ExpansionDataSetup.ExpansionData data)
        {
            List<string> paths = new();
            for (int i = 0; i < data.PuzzleLevels.Count; i++)
            {
                PuzzleLevelDefinition level = data.PuzzleLevels[i];
                if (level == null) continue;
                string sceneName = TW08PuzzleSceneBuilder.ResolveSceneName(level, i + 1);
                paths.Add(TW08PuzzleSceneBuilder.SceneRoot + "/" + sceneName + ".unity");
            }
            return paths;
        }

        private static List<string> GetRacePaths(TW08ExpansionDataSetup.ExpansionData data)
        {
            return data.RaceTracks
                .Where(track => track != null && !string.IsNullOrWhiteSpace(track.SceneName))
                .Select(track => TW08RaceSceneBuilder.SceneRoot + "/" + track.SceneName + ".unity")
                .ToList();
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

            EditorBuildSettingsScene[] entries = ordered
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();

            // Unity 6 can switch the active Build Profile (Player tests are one example). Register the
            // production scenes in the shared/global list as the stable source of truth, then mirror
            // them into the currently active build profile when that profile overrides global scenes.
            EditorBuildSettings.globalScenes = entries;

            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
            if (activeProfile != null)
            {
                activeProfile.overrideGlobalScenes = true;
                activeProfile.scenes = entries;
                EditorUtility.SetDirty(activeProfile);
            }

            // This property targets the active platform/build profile and keeps the current Editor
            // session immediately consistent with the shared scene list.
            EditorBuildSettings.scenes = entries;
            AssetDatabase.SaveAssets();
            ValidateSceneRegistration(entries);
        }

        private static void ValidateSceneRegistration(IEnumerable<EditorBuildSettingsScene> expectedScenes)
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

            List<string> missing = expectedScenes
                .Where(scene => scene != null && scene.enabled)
                .Select(scene => scene.path)
                .Where(path => !global.Contains(path) && !active.Contains(path))
                .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "As seguintes cenas não ficaram registradas nem na Scene List compartilhada nem no perfil ativo:\n- " +
                    string.Join("\n- ", missing));
            }
        }

        private static void EnsureAudioListeners(IEnumerable<string> scenePaths)
        {
            foreach (string path in scenePaths
                         .Where(path => !string.IsNullOrWhiteSpace(path))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;

                Scene scene = SceneManager.GetSceneByPath(path);
                bool openedHere = !scene.IsValid() || !scene.isLoaded;
                if (openedHere)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }

                try
                {
                    if (FindComponentInScene<AudioListener>(scene) != null) continue;

                    Camera camera = FindComponentInScene<Camera>(scene);
                    if (camera == null)
                    {
                        Debug.LogWarning($"TW08: cena '{path}' não possui Camera para receber AudioListener.");
                        continue;
                    }

                    camera.gameObject.AddComponent<AudioListener>();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                finally
                {
                    if (openedHere && scene.IsValid() && scene.isLoaded)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
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