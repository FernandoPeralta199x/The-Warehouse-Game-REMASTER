#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Data;
using TW08.Input;
using TW08.Presentation;
using TW08.Puzzle;
using TW08.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    internal static class TW08PuzzleSceneBuilder
    {
        internal const string SceneRoot = "Assets/_Project/Scenes/VerticalSlice";
        internal const string SecretSceneRoot = "Assets/_Project/Scenes/Secret";

        internal static List<string> BuildAll(TW08ExpansionDataSetup.ExpansionData data, TW08ArtCatalog catalog)
        {
            TW08ProductionSceneUtility.EnsureFolder(SceneRoot);

            // ExpansionData contains UnityEngine.Object references. Scene authoring/import callbacks can
            // invalidate those native handles while this builder is running, even though the serialized
            // assets still exist. Capture only stable asset paths before creating any scene, then reload
            // every Unity object immediately before the scene that consumes it.
            List<PuzzleSceneSpec> specs = LoadBuildSpecs();
            if (specs.Count == 0)
            {
                throw new InvalidOperationException(
                    "Puzzle scene builder resolved zero stable level specs from the campaign.");
            }

            List<string> paths = new();
            foreach (PuzzleSceneSpec spec in specs)
            {
                PuzzleLevelDefinition level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(spec.LevelAssetPath);
                CharacterRoster roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(TW08ExpansionDataSetup.RosterPath);
                TW08ArtCatalog liveCatalog = AssetDatabase.LoadAssetAtPath<TW08ArtCatalog>(TW08ProductionArtSetup.CatalogPath);

                if (level == null)
                {
                    throw new InvalidOperationException(
                        $"Puzzle scene builder could not reload level asset '{spec.LevelAssetPath}'.");
                }
                if (roster == null)
                {
                    throw new InvalidOperationException(
                        $"Puzzle scene builder could not reload character roster '{TW08ExpansionDataSetup.RosterPath}'.");
                }
                if (liveCatalog == null)
                {
                    throw new InvalidOperationException(
                        $"Puzzle scene builder could not reload art catalog '{TW08ProductionArtSetup.CatalogPath}'.");
                }

                string path = SceneRoot + "/" + spec.SceneName + ".unity";
                Build(level, roster, liveCatalog, path, spec.NextSceneName);
                paths.Add(path);
            }

            if (paths.Count != specs.Count)
            {
                throw new InvalidOperationException(
                    $"Puzzle scene builder produced {paths.Count}/{specs.Count} scene paths.");
            }

            return paths;
        }

        /// <summary>
        /// Constrói cenas para uma campanha arbitrária (ex.: TW08_SecretCampaign)
        /// num diretório próprio. Última fase volta para a cena de fallback.
        /// </summary>
        internal static List<string> BuildForCampaign(
            PuzzleCampaignDefinition campaign,
            string sceneRoot,
            string fallbackScene = "TW08_PuzzleSelect")
        {
            if (campaign == null)
            {
                throw new ArgumentNullException(nameof(campaign));
            }

            TW08ProductionSceneUtility.EnsureFolder(sceneRoot);
            List<PuzzleSceneSpec> specs = LoadSpecsFrom(campaign, fallbackScene);
            List<string> paths = new();
            foreach (PuzzleSceneSpec spec in specs)
            {
                PuzzleLevelDefinition level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(spec.LevelAssetPath);
                CharacterRoster roster = AssetDatabase.LoadAssetAtPath<CharacterRoster>(TW08ExpansionDataSetup.RosterPath);
                TW08ArtCatalog liveCatalog = AssetDatabase.LoadAssetAtPath<TW08ArtCatalog>(TW08ProductionArtSetup.CatalogPath);
                if (level == null || roster == null || liveCatalog == null)
                {
                    throw new InvalidOperationException(
                        $"Secret scene builder could not reload dependencies for '{spec.LevelAssetPath}'.");
                }

                string path = sceneRoot + "/" + spec.SceneName + ".unity";
                Build(level, roster, liveCatalog, path, spec.NextSceneName);
                paths.Add(path);
            }

            return paths;
        }

        private static List<PuzzleSceneSpec> LoadBuildSpecs()
        {
            PuzzleCampaignDefinition campaign =
                AssetDatabase.LoadAssetAtPath<PuzzleCampaignDefinition>(TW08ExpansionDataSetup.PuzzleCampaignPath);
            if (campaign == null)
            {
                throw new InvalidOperationException(
                    $"Puzzle campaign could not be loaded from '{TW08ExpansionDataSetup.PuzzleCampaignPath}'.");
            }

            return LoadSpecsFrom(campaign, "TW08_PuzzleSelect");
        }

        private static List<PuzzleSceneSpec> LoadSpecsFrom(PuzzleCampaignDefinition campaign, string fallbackScene)
        {
            List<PuzzleSceneSpec> specs = new(campaign.Levels.Count);
            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                PuzzleCampaignEntry entry = campaign.Levels[i];
                if (entry == null || entry.Level == null)
                {
                    throw new InvalidOperationException($"Puzzle campaign entry {i + 1:00} has no valid level asset.");
                }

                string levelPath = AssetDatabase.GetAssetPath(entry.Level);
                if (string.IsNullOrWhiteSpace(levelPath))
                {
                    throw new InvalidOperationException(
                        $"Puzzle campaign entry {i + 1:00} does not resolve to a persistent asset path.");
                }

                string sceneName = !string.IsNullOrWhiteSpace(entry.SceneName)
                    ? entry.SceneName
                    : ResolveSceneName(entry.Level, i + 1);
                string nextScene = i + 1 < campaign.Levels.Count
                    ? ResolveEntrySceneName(campaign.Levels[i + 1], i + 2)
                    : fallbackScene;

                specs.Add(new PuzzleSceneSpec(levelPath, sceneName, nextScene));
            }

            return specs;
        }

        private static string ResolveEntrySceneName(PuzzleCampaignEntry entry, int index)
        {
            if (entry == null || entry.Level == null)
            {
                throw new InvalidOperationException($"Puzzle campaign entry {index:00} is invalid while resolving progression.");
            }

            return !string.IsNullOrWhiteSpace(entry.SceneName)
                ? entry.SceneName
                : ResolveSceneName(entry.Level, index);
        }

        private static void Build(
            PuzzleLevelDefinition level,
            CharacterRoster roster,
            TW08ArtCatalog catalog,
            string path,
            string nextScene)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameInput input = new GameObject("Game Input").AddComponent<GameInput>();
            PuzzleRuntime runtime = new GameObject("Puzzle Runtime").AddComponent<PuzzleRuntime>();

            DrawBoard(level, catalog, out PuzzleMechanicView mechanicView);

            GameObject john = TW08ProductionSceneUtility.CreateSprite(
                "John",
                level.PlayerStart.ToWorld(level.CellSize),
                catalog.John != null ? catalog.John.GetIdle(FacingDirection.Down) : null,
                30,
                Color.white);
            PuzzleEntityView playerView = john.AddComponent<PuzzleEntityView>();
            playerView.Configure("player", PuzzleEntityKind.Player);
            PuzzlePlayerController controller = john.AddComponent<PuzzlePlayerController>();
            controller.Configure(input, runtime);
            DirectionalSpriteAnimator animator = john.AddComponent<DirectionalSpriteAnimator>();
            animator.Configure(john.GetComponent<SpriteRenderer>(), catalog.John);
            PuzzleCharacterAnimationBinder binder = john.AddComponent<PuzzleCharacterAnimationBinder>();
            binder.Configure(runtime, animator);
            SelectedCharacterPresenter selectedPresenter = john.AddComponent<SelectedCharacterPresenter>();
            selectedPresenter.Configure(roster, animator, john.GetComponent<SpriteRenderer>());

            List<PuzzleEntityView> crateViews = new();
            foreach (PuzzleCrateDefinition crate in level.Crates)
            {
                GameObject crateObject = TW08ProductionSceneUtility.CreateSprite(
                    crate.Id,
                    crate.Position.ToWorld(level.CellSize),
                    catalog.CrateDefault,
                    20,
                    TW08ProductionSceneUtility.CrateTint(crate.Kind));
                PuzzleEntityView view = crateObject.AddComponent<PuzzleEntityView>();
                view.Configure(crate.Id, crate.Kind);
                crateViews.Add(view);
                EditorUtility.SetDirty(view);
            }

            runtime.Configure(level, playerView, crateViews);
            controller.Configure(input, runtime);
            mechanicView.Configure(runtime);

            CreateCamera(level);
            CreateHud(level, runtime, nextScene);

            EditorUtility.SetDirty(runtime);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(binder);
            EditorUtility.SetDirty(selectedPresenter);
            EditorUtility.SetDirty(mechanicView);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new InvalidOperationException($"Unity failed to save generated puzzle scene '{path}'.");
            }
        }

        private static void DrawBoard(PuzzleLevelDefinition level, TW08ArtCatalog catalog, out PuzzleMechanicView mechanicView)
        {
            HashSet<GridCoordinate> walls = new(level.Walls);
            HashSet<GridCoordinate> costly = new(level.CostlyCells ?? Array.Empty<GridCoordinate>());
            Sprite ice = TW08ExpansionStarterArt.LoadRaceSprite("Ice");

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    GridCoordinate cell = new(x, y);
                    Vector3 world = cell.ToWorld(level.CellSize);
                    if (walls.Contains(cell))
                    {
                        TW08ProductionSceneUtility.CreateSprite("Wall " + cell, world, catalog.Wall, 10, Color.white);
                        continue;
                    }

                    Sprite floor = (x + y) % 2 == 0 ? catalog.FloorPrimary : catalog.FloorSecondary;
                    TW08ProductionSceneUtility.CreateSprite($"Floor {x},{y}", world, floor, -20, Color.white);
                    if (costly.Contains(cell) && ice != null)
                    {
                        TW08ProductionSceneUtility.CreateSprite("Cold " + cell, world, ice, -10, new Color(0.72f, 0.94f, 1f, 0.62f));
                    }
                }
            }

            foreach (GridCoordinate goal in level.Goals)
            {
                Color tint = GoalTint(level, goal);
                TW08ProductionSceneUtility.CreateSprite("Goal " + goal, goal.ToWorld(level.CellSize), catalog.Goal, 4, tint, Vector3.one * 0.9f);
            }

            GameObject mechanics = new("Puzzle Mechanics");
            mechanicView = mechanics.AddComponent<PuzzleMechanicView>();

            foreach (PuzzleSwitchGroupDefinition group in level.SwitchGroups ?? Array.Empty<PuzzleSwitchGroupDefinition>())
            {
                if (group == null) continue;
                foreach (GridCoordinate sensor in group.Sensors ?? Array.Empty<GridCoordinate>())
                {
                    GameObject sensorObject = TW08ProductionSceneUtility.CreateSprite(
                        $"Sensor {group.Id} {sensor}", sensor.ToWorld(level.CellSize), catalog.Goal, 6,
                        new Color(0.24f, 0.85f, 0.96f, 0.86f), Vector3.one * 0.72f);
                    sensorObject.transform.SetParent(mechanics.transform);
                }

                foreach (GridCoordinate door in group.Doors ?? Array.Empty<GridCoordinate>())
                {
                    GameObject doorObject = TW08ProductionSceneUtility.CreateSprite(
                        $"Door {group.Id} {door}", door.ToWorld(level.CellSize), catalog.Wall, 12,
                        new Color(1f, 0.61f, 0.16f, 1f));
                    doorObject.transform.SetParent(mechanics.transform);
                    mechanicView.RegisterDoor(group.Id, doorObject);
                }
            }
        }

        private static Color GoalTint(PuzzleLevelDefinition level, GridCoordinate goal)
        {
            PuzzleGoalRequirementDefinition requirement = level.GoalRequirements
                .FirstOrDefault(item => item != null && item.Position == goal);
            if (requirement == null) return Color.white;
            return requirement.RequiredKind switch
            {
                PuzzleEntityKind.HeavyCrate => new Color(0.45f, 0.78f, 1f, 1f),
                PuzzleEntityKind.FragileCrate => new Color(1f, 0.48f, 0.34f, 1f),
                _ => new Color(0.58f, 1f, 0.72f, 1f)
            };
        }

        private static void CreateCamera(PuzzleLevelDefinition level)
        {
            float centerX = (level.Width - 1) * level.CellSize * 0.5f;
            float centerY = (level.Height - 1) * level.CellSize * 0.5f;
            Camera camera = TW08ProductionSceneUtility.CreateCamera(
                new Vector3(centerX, centerY, -10f),
                Mathf.Max(4.2f, level.Height * 0.67f));
            camera.backgroundColor = TW08ProductionSceneUtility.Background;
        }

        private static void CreateHud(PuzzleLevelDefinition level, PuzzleRuntime runtime, string nextScene)
        {
            Canvas canvas = TW08ProductionSceneUtility.CreateCanvas();
            EventSystem eventSystem = TW08ProductionSceneUtility.CreateEventSystem();

            Image top = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "HUD Top", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(top.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -112f), new Vector2(-18f, -18f));

            Text levelText = TW08ProductionSceneUtility.CreateText(top.transform, "Level", level.DisplayName.ToUpperInvariant(), 24, TW08ProductionSceneUtility.TextPrimary, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(levelText.rectTransform, new Vector2(0f, 0.5f), new Vector2(520f, 40f), new Vector2(26f, 24f));

            Text moves = TW08ProductionSceneUtility.CreateText(top.transform, "Moves", "MOVIMENTOS 000   UNDO 00   REDO 00", 16, TW08ProductionSceneUtility.Green, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(moves.rectTransform, new Vector2(0f, 0.5f), new Vector2(620f, 34f), new Vector2(26f, -22f));

            Text operatorText = TW08ProductionSceneUtility.CreateText(top.transform, "Operator", "OPERADOR // JOHN", 16, TW08ProductionSceneUtility.Amber, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(operatorText.rectTransform, new Vector2(1f, 0.5f), new Vector2(420f, 34f), new Vector2(-28f, 24f));

            Text targetText = TW08ProductionSceneUtility.CreateText(top.transform, "Targets", "PLAT 000 // GOLD 000", 14, TW08ProductionSceneUtility.TextMuted, TextAnchor.MiddleRight);
            TW08ProductionSceneUtility.SetRect(targetText.rectTransform, new Vector2(1f, 0.5f), new Vector2(420f, 30f), new Vector2(-28f, -22f));

            Image bottom = TW08ProductionSceneUtility.CreatePanel(canvas.transform, "HUD Bottom", TW08ProductionSceneUtility.Panel);
            TW08ProductionSceneUtility.SetRect(bottom.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 18f), new Vector2(-18f, 108f));

            Text status = TW08ProductionSceneUtility.CreateText(bottom.transform, "Status", "ROTA ATIVA", 16, TW08ProductionSceneUtility.Green, TextAnchor.MiddleLeft);
            TW08ProductionSceneUtility.SetRect(status.rectTransform, new Vector2(0f, 0.5f), new Vector2(650f, 42f), new Vector2(26f, 0f));

            Text briefing = TW08ProductionSceneUtility.CreateText(bottom.transform, "Briefing", level.Briefing, 13, TW08ProductionSceneUtility.TextMuted, TextAnchor.MiddleCenter);
            TW08ProductionSceneUtility.SetRect(briefing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(650f, 58f), Vector2.zero);

            Button undo = TW08ProductionSceneUtility.CreateButton(bottom.transform, "Undo", "UNDO [Z]", TW08ProductionSceneUtility.Cyan, 14);
            Button redo = TW08ProductionSceneUtility.CreateButton(bottom.transform, "Redo", "REDO [Y]", TW08ProductionSceneUtility.Cyan, 14);
            Button action = TW08ProductionSceneUtility.CreateButton(bottom.transform, "Primary Action", "RESET [R]", TW08ProductionSceneUtility.Amber, 14);
            TW08ProductionSceneUtility.SetRect((RectTransform)undo.transform, new Vector2(1f, 0.5f), new Vector2(130f, 48f), new Vector2(-430f, 0f));
            TW08ProductionSceneUtility.SetRect((RectTransform)redo.transform, new Vector2(1f, 0.5f), new Vector2(130f, 48f), new Vector2(-285f, 0f));
            TW08ProductionSceneUtility.SetRect((RectTransform)action.transform, new Vector2(1f, 0.5f), new Vector2(180f, 48f), new Vector2(-90f, 0f));
            TW08ProductionSceneUtility.DisableNavigation(undo);
            TW08ProductionSceneUtility.DisableNavigation(redo);

            PuzzleHudController hud = new GameObject("Puzzle HUD Controller").AddComponent<PuzzleHudController>();
            hud.Configure(runtime, levelText, moves, status, undo, redo, action);
            hud.ConfigureExtendedLabels(operatorText, targetText);
            hud.ConfigureCampaignFlow(nextScene, "TW08_PuzzleSelect");
            TW08ProductionSceneUtility.Select(eventSystem, action);
            EditorUtility.SetDirty(hud);
        }

        internal static string ResolveSceneName(PuzzleLevelDefinition level, int index)
        {
            if (index == 1) return "TW08_Level01_FirstShift";
            if (index == 2) return "TW08_Level02_TightCorridor";
            if (index == 3) return "TW08_Level03_CrossLoad";
            return level != null && !string.IsNullOrWhiteSpace(level.LevelId) ? level.LevelId : $"TW08_Level{index:00}";
        }

        private readonly struct PuzzleSceneSpec
        {
            public PuzzleSceneSpec(string levelAssetPath, string sceneName, string nextSceneName)
            {
                LevelAssetPath = levelAssetPath;
                SceneName = sceneName;
                NextSceneName = nextSceneName;
            }

            public string LevelAssetPath { get; }
            public string SceneName { get; }
            public string NextSceneName { get; }
        }
    }
}
#endif
