#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Editor
{
    internal static class TW08BasePuzzleDataRecovery
    {
        private const string Root = "Assets/_Project/ScriptableObjects/VerticalSlice";

        internal static void EnsureBaseLevels()
        {
            TW08ProductionSceneUtility.EnsureFolder("Assets/_Project/ScriptableObjects");
            TW08ProductionSceneUtility.EnsureFolder(Root);
            EnsureLevel(
                Root + "/TW08_Level01_FirstShift.asset",
                "TW08_Level01_FirstShift", "Primeiro Turno", 7, 5, new Vector2Int(1, 2),
                new[] { new Vector2Int(5, 2) },
                new[] { new CrateSpec("crate-a", 3, 2) },
                new Vector2Int[0], 5, 3,
                "Primeiro turno no N-8. Leve a carga até a doca e confirme o fluxo básico.");

            EnsureLevel(
                Root + "/TW08_Level02_TightCorridor.asset",
                "TW08_Level02_TightCorridor", "Corredor Apertado", 8, 7, new Vector2Int(1, 1),
                new[] { new Vector2Int(2, 5), new Vector2Int(6, 1) },
                new[] { new CrateSpec("crate-a", 2, 2), new CrateSpec("crate-b", 5, 4) },
                new[] { new Vector2Int(4, 1), new Vector2Int(4, 2), new Vector2Int(4, 3) }, 16, 12,
                "Corredores estreitos punem empurrões sem rota de retorno. Caixas não podem ser puxadas.");

            EnsureLevel(
                Root + "/TW08_Level03_CrossLoad.asset",
                "TW08_Level03_CrossLoad", "Carga Cruzada", 9, 7, new Vector2Int(1, 3),
                new[] { new Vector2Int(1, 1), new Vector2Int(7, 3), new Vector2Int(7, 5) },
                new[] { new CrateSpec("crate-a", 3, 2), new CrateSpec("crate-b", 4, 3), new CrateSpec("crate-c", 5, 4) },
                new[] { new Vector2Int(4, 1), new Vector2Int(4, 5), new Vector2Int(2, 3), new Vector2Int(6, 3) }, 40, 31,
                "A ordem das cargas define o espaço disponível. Abra corredores antes de comprometer a rota.");
            AssetDatabase.SaveAssets();
        }

        private static void EnsureLevel(
            string path, string id, string displayName, int width, int height, Vector2Int player,
            IEnumerable<Vector2Int> goals, IReadOnlyList<CrateSpec> crates, IEnumerable<Vector2Int> internalWalls,
            int gold, int platinum, string briefing)
        {
            PuzzleLevelDefinition level = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }

            SerializedObject s = new(level);
            s.FindProperty("levelId").stringValue = id;
            s.FindProperty("displayName").stringValue = displayName;
            s.FindProperty("sectorId").stringValue = "S01";
            s.FindProperty("briefing").stringValue = briefing;
            s.FindProperty("width").intValue = width;
            s.FindProperty("height").intValue = height;
            s.FindProperty("cellSize").floatValue = 1f;
            s.FindProperty("goldMoveLimit").intValue = gold;
            s.FindProperty("platinumMoveLimit").intValue = platinum;
            s.FindProperty("allowPowerUps").boolValue = false;
            SetCoordinate(s.FindProperty("playerStart"), player.x, player.y);

            HashSet<Vector2Int> walls = new();
            for (int x = 0; x < width; x++)
            {
                walls.Add(new Vector2Int(x, 0));
                walls.Add(new Vector2Int(x, height - 1));
            }
            for (int y = 0; y < height; y++)
            {
                walls.Add(new Vector2Int(0, y));
                walls.Add(new Vector2Int(width - 1, y));
            }
            foreach (Vector2Int wall in internalWalls) walls.Add(wall);
            SetCoordinates(s.FindProperty("walls"), walls);
            SetCoordinates(s.FindProperty("goals"), goals);
            s.FindProperty("goalRequirements").arraySize = 0;
            s.FindProperty("costlyCells").arraySize = 0;
            s.FindProperty("switchGroups").arraySize = 0;
            s.FindProperty("gimmickTags").arraySize = 0;

            SerializedProperty crateList = s.FindProperty("crates");
            crateList.arraySize = crates.Count;
            for (int i = 0; i < crates.Count; i++)
            {
                SerializedProperty item = crateList.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("id").stringValue = crates[i].Id;
                item.FindPropertyRelative("kind").enumValueIndex = (int)PuzzleEntityKind.Crate;
                SetCoordinate(item.FindPropertyRelative("position"), crates[i].X, crates[i].Y);
            }
            s.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
        }

        private static void SetCoordinates(SerializedProperty property, IEnumerable<Vector2Int> values)
        {
            Vector2Int[] cells = values.ToArray();
            property.arraySize = cells.Length;
            for (int i = 0; i < cells.Length; i++) SetCoordinate(property.GetArrayElementAtIndex(i), cells[i].x, cells[i].y);
        }

        private static void SetCoordinate(SerializedProperty property, int x, int y)
        {
            property.FindPropertyRelative("x").intValue = x;
            property.FindPropertyRelative("y").intValue = y;
        }

        private readonly struct CrateSpec
        {
            public CrateSpec(string id, int x, int y) { Id = id; X = x; Y = y; }
            public string Id { get; }
            public int X { get; }
            public int Y { get; }
        }
    }
}
#endif
