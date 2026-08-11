#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TW08.Input;
using TW08.Puzzle;
using TW08.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TW08.Editor
{
    public static class TW08VerticalSliceSetup
    {
        private const string DataRoot = "Assets/_Project/ScriptableObjects/VerticalSlice";
        private const string SceneRoot = "Assets/_Project/Scenes/VerticalSlice";
        private static readonly Color Background = new(0.025f, 0.035f, 0.045f, 1f);
        private static readonly Color Panel = new(0.055f, 0.075f, 0.085f, 0.96f);
        private static readonly Color Accent = new(0.2f, 0.92f, 0.62f, 1f);
        private static readonly Color Amber = new(1f, 0.68f, 0.18f, 1f);
        private static readonly Color TextPrimary = new(0.88f, 0.96f, 0.92f, 1f);
        private static readonly Color TextMuted = new(0.52f, 0.68f, 0.62f, 1f);

        [MenuItem("Tools/TW08/Create Professional Vertical Slice")]
        public static void CreateProfessionalVerticalSlice()
        {
            EnsureFolder(DataRoot);
            EnsureFolder(SceneRoot);

            PuzzleLevelDefinition level01 = CreateLevel01();
            PuzzleLevelDefinition level02 = CreateLevel02();
            PuzzleLevelDefinition level03 = CreateLevel03();
            AssetDatabase.SaveAssets();

            string menuPath = $"{SceneRoot}/TW08_MainMenu.unity";
            string level01Path = $"{SceneRoot}/TW08_Level01_FirstShift.unity";
            string level02Path = $"{SceneRoot}/TW08_Level02_TightCorridor.unity";
            string level03Path = $"{SceneRoot}/TW08_Level03_CrossLoad.unity";

            CreateMainMenuScene(menuPath);
            CreatePuzzleScene(level01, level01Path, "WASD / SETAS  MOVER     Z  UNDO     Y  REDO     R  REINICIAR");
            CreatePuzzleScene(level02, level02Path, "PLANEJE ANTES DE EMPURRAR // CAIXAS NÃO PODEM SER PUXADAS");
            CreatePuzzleScene(level03, level03Path, "ABRA ESPAÇO LATERAL // A ORDEM DAS CARGAS IMPORTA");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(menuPath, true),
                new EditorBuildSettingsScene(level01Path, true),
                new EditorBuildSettingsScene(level02Path, true),
                new EditorBuildSettingsScene(level03Path, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(menuPath, OpenSceneMode.Single);
            EditorUtility.DisplayDialog(
                "The Warehouse Nº 08",
                "Vertical slice criado: menu + 3 fases originais + HUD + input teclado/gamepad. Rode o Test Runner antes de considerar o slice validado.",
                "OK");
        }

        private static PuzzleLevelDefinition CreateLevel01()
        {
            return ConfigureLevel(
                "TW08_Level01_FirstShift",
                "Primeiro Turno",
                7,
                5,
                new Vector2Int(1, 2),
                BuildBorder(7, 5),
                new[] { new Vector2Int(5, 2) },
                new[] { new CrateSpec("crate-a", 3, 2) },
                5,
                3);
        }

        private static PuzzleLevelDefinition CreateLevel02()
        {
            HashSet<Vector2Int> walls = new(BuildBorder(8, 7))
            {
                new(4, 1),
                new(4, 2),
                new(4, 3)
            };

            return ConfigureLevel(
                "TW08_Level02_TightCorridor",
                "Corredor Apertado",
                8,
                7,
                new Vector2Int(1, 1),
                walls,
                new[] { new Vector2Int(2, 5), new Vector2Int(6, 1) },
                new[] { new CrateSpec("crate-a", 2, 2), new CrateSpec("crate-b", 5, 4) },
                16,
                12);
        }

        private static PuzzleLevelDefinition CreateLevel03()
        {
            HashSet<Vector2Int> walls = new(BuildBorder(9, 7))
            {
                new(4, 1),
                new(4, 5),
                new(2, 3),
                new(6, 3)
            };

            return ConfigureLevel(
                "TW08_Level03_CrossLoad",
                "Carga Cruzada",
                9,
                7,
                new Vector2Int(1, 3),
                walls,
                new[] { new Vector2Int(1, 1), new Vector2Int(7, 3), new Vector2Int(7, 5) },
                new[]
                {
                    new CrateSpec("crate-a", 3, 2),
                    new CrateSpec("crate-b", 4, 3),
                    new CrateSpec("crate-c", 5, 4)
                },
                40,
                31);
        }

        private static PuzzleLevelDefinition ConfigureLevel(
            string id,
            string displayName,
            int width,
            int height,
            Vector2Int player,
            IEnumerable<Vector2Int> walls,
            IEnumerable<Vector2Int> goals,
            IReadOnlyList<CrateSpec> crates,
            int gold,
            int platinum)
        {
            string path = $"{DataRoot}/{id}.asset";
            PuzzleLevelDefinition level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }

            SerializedObject serialized = new(level);
            serialized.FindProperty("levelId").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("width").intValue = width;
            serialized.FindProperty("height").intValue = height;
            serialized.FindProperty("cellSize").floatValue = 1f;
            serialized.FindProperty("allowPowerUps").boolValue = false;
            SetCoordinate(serialized.FindProperty("playerStart"), player.x, player.y);
            SetCoordinates(serialized.FindProperty("walls"), walls);
            SetCoordinates(serialized.FindProperty("goals"), goals);

            SerializedProperty crateList = serialized.FindProperty("crates");
            crateList.arraySize = crates.Count;
            for (int i = 0; i < crates.Count; i++)
            {
                CrateSpec crate = crates[i];
                SerializedProperty item = crateList.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue = crate.Id;
                item.FindPropertyRelative("kind").enumValueIndex = (int)PuzzleEntityKind.Crate;
                SetCoordinate(item.FindPropertyRelative("position"), crate.X, crate.Y);
            }

            serialized.FindProperty("goldMoveLimit").intValue = gold;
            serialized.FindProperty("platinumMoveLimit").intValue = platinum;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            return level;
        }

        private static void CreateMainMenuScene(string path)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera(new Vector3(0f, 0f, -10f), 5f);
            camera.backgroundColor = Background;

            Canvas canvas = CreateCanvas();
            CreateEventSystem();

            Image backdrop = CreatePanel(canvas.transform, "Backdrop", Background);
            Stretch(backdrop.rectTransform);

            Image shell = CreatePanel(canvas.transform, "Terminal Shell", Panel);
            SetRect(shell.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(820f, 620f), Vector2.zero);

            Text eyebrow = CreateText(shell.transform, "Eyebrow", "N-8 LOGISTICS // MANUAL RECOVERY TERMINAL", 18, Accent, TextAnchor.MiddleCenter);
            SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(720f, 36f), new Vector2(0f, -58f));

            Text title = CreateText(shell.transform, "Title", "THE WAREHOUSE\nNº 08", 58, TextPrimary, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(720f, 150f), new Vector2(0f, -145f));

            Text subtitle = CreateText(shell.transform, "Subtitle", "OPERADOR MANUAL NECESSÁRIO", 20, Amber, TextAnchor.MiddleCenter);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(620f, 40f), new Vector2(0f, -245f));

            GameObject controllerObject = new("Main Menu Controller");
            RetroMainMenuController controller = controllerObject.AddComponent<RetroMainMenuController>();

            Button newShift = CreateButton(shell.transform, "New Shift", "INICIAR NOVO TURNO", Accent);
            SetRect((RectTransform)newShift.transform, new Vector2(0.5f, 0.5f), new Vector2(430f, 58f), new Vector2(0f, 40f));
            UnityEventTools.AddPersistentListener(newShift.onClick, controller.StartNewShift);

            Button continueShift = CreateButton(shell.transform, "Continue", "CONTINUAR [EM BREVE]", TextMuted);
            continueShift.interactable = false;
            SetRect((RectTransform)continueShift.transform, new Vector2(0.5f, 0.5f), new Vector2(430f, 58f), new Vector2(0f, -30f));
            controller.Configure(newShift, continueShift);

            Button quit = CreateButton(shell.transform, "Quit", "ENCERRAR TERMINAL", Amber);
            SetRect((RectTransform)quit.transform, new Vector2(0.5f, 0.5f), new Vector2(430f, 58f), new Vector2(0f, -100f));
            UnityEventTools.AddPersistentListener(quit.onClick, controller.QuitGame);

            Text footer = CreateText(shell.transform, "Footer", "WASD / SETAS / D-PAD     ENTER / A CONFIRMAR", 15, TextMuted, TextAnchor.MiddleCenter);
            SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(700f, 36f), new Vector2(0f, 42f));

            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreatePuzzleScene(PuzzleLevelDefinition level, string path, string hint)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameInput input = new GameObject("Game Input").AddComponent<GameInput>();
            PuzzleRuntime runtime = new GameObject("Puzzle Runtime").AddComponent<PuzzleRuntime>();

            DrawFloor(level);
            foreach (GridCoordinate wall in level.Walls)
            {
                CreateSquare($"Wall {wall}", wall.ToWorld(level.CellSize), new Color(0.13f, 0.18f, 0.19f), Vector2.one * 0.96f, 1);
            }
            foreach (GridCoordinate goal in level.Goals)
            {
                CreateSquare($"Goal {goal}", goal.ToWorld(level.CellSize), new Color(0.1f, 0.8f, 0.53f, 0.68f), Vector2.one * 0.58f, 2);
            }

            PuzzleEntityView player = CreateEntity("John", "player", PuzzleEntityKind.Player, Amber, 5);
            List<PuzzleEntityView> crates = level.Crates
                .Select(crate => CreateEntity(crate.Id, crate.Id, crate.Kind, new Color(0.72f, 0.39f, 0.12f), 4))
                .ToList();

            runtime.Configure(level, player, crates);
            EditorUtility.SetDirty(runtime);

            PuzzlePlayerController controller = player.gameObject.AddComponent<PuzzlePlayerController>();
            controller.Configure(input, runtime);
            EditorUtility.SetDirty(controller);

            player.Snap(level.PlayerStart, level.CellSize);
            EditorUtility.SetDirty(player);
            for (int i = 0; i < level.Crates.Count && i < crates.Count; i++)
            {
                crates[i].Snap(level.Crates[i].Position, level.CellSize);
                EditorUtility.SetDirty(crates[i]);
            }

            Camera camera = CreateCamera(
                new Vector3((level.Width - 1) * 0.5f, (level.Height - 1) * 0.5f, -10f),
                Mathf.Max(3.6f, level.Height * 0.62f));
            camera.backgroundColor = Background;

            Canvas canvas = CreateCanvas();
            CreateEventSystem();
            CreatePuzzleHud(canvas.transform, runtime, hint);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void DrawFloor(PuzzleLevelDefinition level)
        {
            HashSet<GridCoordinate> walls = new(level.Walls);
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    GridCoordinate cell = new(x, y);
                    if (walls.Contains(cell))
                    {
                        continue;
                    }

                    bool alternate = (x + y) % 2 == 0;
                    Color color = alternate
                        ? new Color(0.045f, 0.065f, 0.07f)
                        : new Color(0.055f, 0.078f, 0.082f);
                    CreateSquare($"Floor {x},{y}", cell.ToWorld(level.CellSize), color, Vector2.one * 0.98f, -2);
                }
            }
        }

        private static void CreatePuzzleHud(Transform parent, PuzzleRuntime runtime, string hint)
        {
            Image top = CreatePanel(parent, "HUD Top", Panel);
            top.rectTransform.anchorMin = new Vector2(0f, 1f);
            top.rectTransform.anchorMax = new Vector2(1f, 1f);
            top.rectTransform.pivot = new Vector2(0.5f, 1f);
            top.rectTransform.sizeDelta = new Vector2(0f, 92f);
            top.rectTransform.anchoredPosition = Vector2.zero;

            Text levelName = CreateText(top.transform, "Level", runtime.Level.DisplayName.ToUpperInvariant(), 24, TextPrimary, TextAnchor.MiddleLeft);
            SetRect(levelName.rectTransform, new Vector2(0f, 0.5f), new Vector2(420f, 42f), new Vector2(32f, 20f));

            Text moves = CreateText(top.transform, "Moves", "MOVIMENTOS 000   UNDO 00   REDO 00", 16, Accent, TextAnchor.MiddleLeft);
            SetRect(moves.rectTransform, new Vector2(0f, 0.5f), new Vector2(500f, 34f), new Vector2(32f, -20f));

            Text status = CreateText(top.transform, "Status", "ROTA ATIVA", 17, Amber, TextAnchor.MiddleRight);
            SetRect(status.rectTransform, new Vector2(1f, 0.5f), new Vector2(450f, 42f), new Vector2(-32f, 10f));

            Image bottom = CreatePanel(parent, "HUD Bottom", Panel);
            bottom.rectTransform.anchorMin = new Vector2(0f, 0f);
            bottom.rectTransform.anchorMax = new Vector2(1f, 0f);
            bottom.rectTransform.pivot = new Vector2(0.5f, 0f);
            bottom.rectTransform.sizeDelta = new Vector2(0f, 88f);
            bottom.rectTransform.anchoredPosition = Vector2.zero;

            Text hintText = CreateText(bottom.transform, "Hint", hint, 14, TextMuted, TextAnchor.MiddleLeft);
            SetRect(hintText.rectTransform, new Vector2(0f, 0.5f), new Vector2(800f, 52f), new Vector2(28f, 0f));

            Button undo = CreateButton(bottom.transform, "Undo", "UNDO [Z]", Accent);
            DisableNavigation(undo);
            SetRect((RectTransform)undo.transform, new Vector2(1f, 0.5f), new Vector2(130f, 44f), new Vector2(-360f, 0f));

            Button redo = CreateButton(bottom.transform, "Redo", "REDO [Y]", Accent);
            DisableNavigation(redo);
            SetRect((RectTransform)redo.transform, new Vector2(1f, 0.5f), new Vector2(130f, 44f), new Vector2(-215f, 0f));

            Button restart = CreateButton(bottom.transform, "Restart", "RESET [R]", Amber);
            DisableNavigation(restart);
            SetRect((RectTransform)restart.transform, new Vector2(1f, 0.5f), new Vector2(130f, 44f), new Vector2(-70f, 0f));

            PuzzleHudController hud = new GameObject("Puzzle HUD Controller").AddComponent<PuzzleHudController>();
            hud.Configure(runtime, levelName, moves, status, undo, redo, restart);
            EditorUtility.SetDirty(hud);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
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

        private static PuzzleEntityView CreateEntity(string name, string id, PuzzleEntityKind kind, Color color, int order)
        {
            GameObject entity = CreateSquare(name, Vector3.zero, color, Vector2.one * 0.76f, order);
            PuzzleEntityView view = entity.AddComponent<PuzzleEntityView>();
            view.Configure(id, kind);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static GameObject CreateSquare(string name, Vector3 position, Color color, Vector2 size, int order)
        {
            GameObject gameObject = new(name);
            gameObject.transform.position = position;
            PrototypeSpriteRenderer renderer = gameObject.AddComponent<PrototypeSpriteRenderer>();
            renderer.Configure(color, size, order);
            return gameObject;
        }

        private static Image CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Color accent)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = new Color(accent.r * 0.18f, accent.g * 0.18f, accent.b * 0.18f, 1f);
            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);
            button.colors = colors;

            Text text = CreateText(go.transform, "Label", label, 17, accent, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void DisableNavigation(Selectable selectable)
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.None;
            selectable.navigation = navigation;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
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

        private readonly struct CrateSpec
        {
            public CrateSpec(string id, int x, int y)
            {
                Id = id;
                X = x;
                Y = y;
            }

            public string Id { get; }
            public int X { get; }
            public int Y { get; }
        }
    }
}
#endif