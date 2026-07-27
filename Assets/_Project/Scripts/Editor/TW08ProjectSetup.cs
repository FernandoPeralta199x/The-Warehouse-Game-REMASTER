#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TW08.Common;
using TW08.Data;
using TW08.Input;
using TW08.PowerUps;
using TW08.Puzzle;
using TW08.Race;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TW08.Editor
{
    public static class TW08ProjectSetup
    {
        private const string DataRoot = "Assets/_Project/ScriptableObjects/Prototype";
        private const string SceneRoot = "Assets/_Project/Scenes/Tests";

        [MenuItem("Tools/TW08/Create Starter Content and Prototype Scenes")]
        public static void CreateStarterContent()
        {
            EnsureFolder(DataRoot);
            EnsureFolder(SceneRoot);

            GameConfig config = CreateOrLoad<GameConfig>($"{DataRoot}/GameConfig.asset");
            PuzzleLevelDefinition puzzleLevel = CreatePuzzleLevel();
            ForkliftStats forkliftStats = CreateOrLoad<ForkliftStats>($"{DataRoot}/ForkliftStats_Standard.asset");
            List<PowerUpDefinition> powerUps = CreatePowerUps();
            WeightedPowerUpTable table = CreatePowerUpTable(powerUps);

            CreatePuzzleScene(puzzleLevel);
            CreateRaceScene(forkliftStats, table);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = config;
            EditorUtility.DisplayDialog("TW08", "Starter assets and prototype scenes created successfully.", "OK");
        }

        private static PuzzleLevelDefinition CreatePuzzleLevel()
        {
            string path = $"{DataRoot}/PuzzleLevel_Prototype.asset";
            PuzzleLevelDefinition level = CreateOrLoad<PuzzleLevelDefinition>(path);
            SerializedObject serialized = new(level);
            serialized.FindProperty("levelId").stringValue = "prototype-001";
            serialized.FindProperty("displayName").stringValue = "Primeiro Turno — Prototype";
            serialized.FindProperty("width").intValue = 8;
            serialized.FindProperty("height").intValue = 6;
            serialized.FindProperty("cellSize").floatValue = 1f;
            SetCoordinate(serialized.FindProperty("playerStart"), 1, 1);
            SetCoordinates(serialized.FindProperty("walls"), BuildBorder(8, 6));
            SetCoordinates(serialized.FindProperty("goals"), new[] { new Vector2Int(5, 1), new Vector2Int(5, 3) });

            SerializedProperty crates = serialized.FindProperty("crates");
            crates.arraySize = 2;
            SetCrate(crates.GetArrayElementAtIndex(0), "crate-a", PuzzleEntityKind.Crate, 2, 1);
            SetCrate(crates.GetArrayElementAtIndex(1), "crate-b", PuzzleEntityKind.Crate, 3, 3);
            serialized.FindProperty("goldMoveLimit").intValue = 35;
            serialized.FindProperty("platinumMoveLimit").intValue = 24;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static List<PowerUpDefinition> CreatePowerUps()
        {
            return new List<PowerUpDefinition>
            {
                CreatePowerUp("TurboCompressor", "turbo-compressor", "Turbo Compressor", PowerUpType.TurboCompressor, 1.45f, 1.1f, 0f),
                CreatePowerUp("SafetyBarrier", "safety-barrier", "Safety Barrier", PowerUpType.SafetyBarrier, 1f, 0f, 0f),
                CreatePowerUp("OilCanister", "oil-canister", "Oil Canister", PowerUpType.OilCanister, 1f, 0f, 0f),
                CreatePowerUp("EmpSignal", "emp-signal", "EMP Signal", PowerUpType.EmpSignal, 0.55f, 1.25f, 5f),
                CreatePowerUp("RepairKit", "repair-kit", "Repair Kit", PowerUpType.RepairKit, 30f, 0f, 0f)
            };
        }

        private static PowerUpDefinition CreatePowerUp(string file, string id, string name, PowerUpType type, float magnitude, float duration, float radius)
        {
            PowerUpDefinition definition = CreateOrLoad<PowerUpDefinition>($"{DataRoot}/PowerUp_{file}.asset");
            SerializedObject serialized = new(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = name;
            serialized.FindProperty("type").enumValueIndex = (int)type;
            serialized.FindProperty("magnitude").floatValue = magnitude;
            serialized.FindProperty("duration").floatValue = duration;
            serialized.FindProperty("radius").floatValue = radius;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static WeightedPowerUpTable CreatePowerUpTable(IReadOnlyList<PowerUpDefinition> definitions)
        {
            WeightedPowerUpTable table = CreateOrLoad<WeightedPowerUpTable>($"{DataRoot}/PowerUpTable_Race.asset");
            SerializedObject serialized = new(table);
            SerializedProperty entries = serialized.FindProperty("entries");
            entries.arraySize = definitions.Count;

            for (int i = 0; i < definitions.Count; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue = definitions[i];
                entry.FindPropertyRelative("minimumRank").floatValue = i == 1 ? 0f : 0.15f;
                entry.FindPropertyRelative("maximumRank").floatValue = i == 4 ? 1f : 0.95f;
                entry.FindPropertyRelative("weight").floatValue = i switch
                {
                    0 => 2.5f,
                    1 => 1.5f,
                    2 => 1.8f,
                    3 => 2.1f,
                    4 => 2.2f,
                    _ => 1f
                };
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            return table;
        }

        private static void CreatePuzzleScene(PuzzleLevelDefinition level)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject inputObject = new("Game Input");
            GameInput input = inputObject.AddComponent<GameInput>();

            GameObject runtimeObject = new("Puzzle Runtime");
            PuzzleRuntime runtime = runtimeObject.AddComponent<PuzzleRuntime>();

            PuzzleEntityView playerView = CreatePuzzleEntity("John", "player", PuzzleEntityKind.Player, Color.yellow, 3);
            List<PuzzleEntityView> crateViews = new()
            {
                CreatePuzzleEntity("Crate A", "crate-a", PuzzleEntityKind.Crate, new Color(0.55f, 0.28f, 0.08f), 2),
                CreatePuzzleEntity("Crate B", "crate-b", PuzzleEntityKind.Crate, new Color(0.65f, 0.35f, 0.1f), 2)
            };

            foreach (GridCoordinate wall in level.Walls)
            {
                CreatePrototypeSquare($"Wall {wall}", wall.ToWorld(level.CellSize), new Color(0.12f, 0.14f, 0.16f), Vector2.one * 0.94f, 1);
            }

            foreach (GridCoordinate goal in level.Goals)
            {
                CreatePrototypeSquare($"Goal {goal}", goal.ToWorld(level.CellSize), new Color(0.1f, 0.65f, 0.45f, 0.7f), Vector2.one * 0.65f, 0);
            }

            runtime.Configure(level, playerView, crateViews);
            PuzzlePlayerController controller = playerView.gameObject.AddComponent<PuzzlePlayerController>();
            controller.Configure(input, runtime);

            Camera camera = CreateCamera(new Vector3(3.5f, 2.5f, -10f), 4.5f);
            camera.backgroundColor = new Color(0.035f, 0.04f, 0.05f);
            runtime.Initialize();
            EditorSceneManager.SaveScene(scene, $"{SceneRoot}/PuzzlePrototype.unity");
        }

        private static void CreateRaceScene(ForkliftStats stats, WeightedPowerUpTable table)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject inputObject = new("Game Input");
            GameInput input = inputObject.AddComponent<GameInput>();

            GameObject managerObject = new("Race Manager");
            RaceManager manager = managerObject.AddComponent<RaceManager>();
            Vector2[] checkpointPositions = { new(0, -6), new(8, 0), new(0, 6), new(-8, 0) };
            List<RaceCheckpoint> checkpoints = new();

            for (int i = 0; i < checkpointPositions.Length; i++)
            {
                GameObject checkpointObject = CreatePrototypeSquare($"Checkpoint {i}", checkpointPositions[i], new Color(0.2f, 0.7f, 1f, 0.22f), new Vector2(4f, 0.5f), 0);
                BoxCollider2D trigger = checkpointObject.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                RaceCheckpoint checkpoint = checkpointObject.AddComponent<RaceCheckpoint>();
                checkpoint.Configure(manager, i);
                checkpoints.Add(checkpoint);
            }

            manager.Configure(checkpoints, 3);
            CreateTrackWalls();
            RaceWaypointPath waypointPath = CreateWaypointPath();

            GameObject forklift = CreatePrototypeSquare("N-8 Standard", new Vector3(0, -4.5f, 0), new Color(0.95f, 0.58f, 0.05f), new Vector2(0.9f, 1.4f), 3);
            Rigidbody2D body = forklift.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 0.15f;
            body.angularDamping = 2f;
            forklift.AddComponent<BoxCollider2D>();
            ArcadeForkliftController2D controller = forklift.AddComponent<ArcadeForkliftController2D>();
            controller.Configure(input, stats, true);
            ForkliftDamage damage = forklift.AddComponent<ForkliftDamage>();
            RacerProgress progress = forklift.AddComponent<RacerProgress>();
            progress.Configure(manager, "player");
            PowerUpInventory inventory = forklift.AddComponent<PowerUpInventory>();
            PowerUpExecutor executor = forklift.AddComponent<PowerUpExecutor>();
            executor.Configure(input, inventory, controller, damage);

            GameObject aiForklift = CreatePrototypeSquare("N-8 AI", new Vector3(1.4f, -4.5f, 0), new Color(0.2f, 0.65f, 0.95f), new Vector2(0.9f, 1.4f), 3);
            Rigidbody2D aiBody = aiForklift.AddComponent<Rigidbody2D>();
            aiBody.gravityScale = 0f;
            aiBody.linearDamping = 0.15f;
            aiBody.angularDamping = 2f;
            aiForklift.AddComponent<BoxCollider2D>();
            ArcadeForkliftController2D aiController = aiForklift.AddComponent<ArcadeForkliftController2D>();
            aiController.Configure(input, stats, false);
            aiForklift.AddComponent<ForkliftDamage>();
            RacerProgress aiProgress = aiForklift.AddComponent<RacerProgress>();
            aiProgress.Configure(manager, "ai-01");
            WaypointRaceAI ai = aiForklift.AddComponent<WaypointRaceAI>();
            ai.Configure(aiController, waypointPath);

            GameObject boost = CreatePrototypeSquare("Boost Pad", new Vector3(4, -5.5f, 0), new Color(0.1f, 0.8f, 0.9f), new Vector2(2f, 1f), 1);
            BoxCollider2D boostCollider = boost.AddComponent<BoxCollider2D>();
            boostCollider.isTrigger = true;
            boost.AddComponent<BoostPad>();

            GameObject pickup = CreatePrototypeSquare("Power Up Pickup", new Vector3(-4, 5.5f, 0), new Color(0.75f, 0.25f, 0.9f), Vector2.one, 2);
            CircleCollider2D pickupCollider = pickup.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            PowerUpPickup pickupComponent = pickup.AddComponent<PowerUpPickup>();
            pickupComponent.Configure(table, manager);

            Camera camera = CreateCamera(new Vector3(0, -4.5f, -10f), 6.5f);
            CameraFollow2D follow = camera.gameObject.AddComponent<CameraFollow2D>();
            follow.Configure(forklift.transform, new Vector3(0, 0, -10));
            camera.backgroundColor = new Color(0.035f, 0.04f, 0.05f);
            EditorSceneManager.SaveScene(scene, $"{SceneRoot}/RacePrototype.unity");
        }

        private static RaceWaypointPath CreateWaypointPath()
        {
            GameObject root = new("Race Waypoints");
            Vector2[] positions =
            {
                new(0, -5.5f),
                new(7.5f, -5.5f),
                new(8.5f, 0),
                new(7.5f, 5.5f),
                new(0, 6.5f),
                new(-7.5f, 5.5f),
                new(-8.5f, 0),
                new(-7.5f, -5.5f)
            };
            List<Transform> points = new();
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new($"Waypoint {i:00}");
                point.transform.SetParent(root.transform);
                point.transform.position = positions[i];
                points.Add(point.transform);
            }

            RaceWaypointPath path = root.AddComponent<RaceWaypointPath>();
            path.Configure(points);
            return path;
        }

        private static void CreateTrackWalls()
        {
            CreateWall("North Wall", new Vector2(0, 8), new Vector2(20, 1));
            CreateWall("South Wall", new Vector2(0, -8), new Vector2(20, 1));
            CreateWall("East Wall", new Vector2(10, 0), new Vector2(1, 17));
            CreateWall("West Wall", new Vector2(-10, 0), new Vector2(1, 17));
            CreateWall("Inner North", new Vector2(0, 3), new Vector2(10, 1));
            CreateWall("Inner South", new Vector2(0, -3), new Vector2(10, 1));
            CreateWall("Inner East", new Vector2(5, 0), new Vector2(1, 7));
            CreateWall("Inner West", new Vector2(-5, 0), new Vector2(1, 7));
        }

        private static void CreateWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = CreatePrototypeSquare(name, position, new Color(0.12f, 0.14f, 0.16f), size, 1);
            wall.AddComponent<BoxCollider2D>();
        }

        private static PuzzleEntityView CreatePuzzleEntity(string name, string id, PuzzleEntityKind kind, Color color, int order)
        {
            GameObject entity = CreatePrototypeSquare(name, Vector3.zero, color, Vector2.one * 0.8f, order);
            PuzzleEntityView view = entity.AddComponent<PuzzleEntityView>();
            view.Configure(id, kind);
            return view;
        }

        private static GameObject CreatePrototypeSquare(string name, Vector3 position, Color color, Vector2 size, int order)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;
            PrototypeSpriteRenderer renderer = gameObject.AddComponent<PrototypeSpriteRenderer>();
            renderer.Configure(color, size, order);
            return gameObject;
        }

        private static Camera CreateCamera(Vector3 position, float size)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = position;
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = size;
            return camera;
        }

        private static IEnumerable<Vector2Int> BuildBorder(int width, int height)
        {
            HashSet<Vector2Int> result = new();
            for (int x = 0; x < width; x++)
            {
                result.Add(new Vector2Int(x, 0));
                result.Add(new Vector2Int(x, height - 1));
            }
            for (int y = 0; y < height; y++)
            {
                result.Add(new Vector2Int(0, y));
                result.Add(new Vector2Int(width - 1, y));
            }
            return result;
        }

        private static void SetCoordinates(SerializedProperty list, IEnumerable<Vector2Int> values)
        {
            Vector2Int[] cells = values.ToArray();
            list.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++)
            {
                SetCoordinate(list.GetArrayElementAtIndex(i), cells[i].x, cells[i].y);
            }
        }

        private static void SetCoordinate(SerializedProperty property, int x, int y)
        {
            property.FindPropertyRelative("x").intValue = x;
            property.FindPropertyRelative("y").intValue = y;
        }

        private static void SetCrate(SerializedProperty property, string id, PuzzleEntityKind kind, int x, int y)
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            SetCoordinate(property.FindPropertyRelative("position"), x, y);
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
