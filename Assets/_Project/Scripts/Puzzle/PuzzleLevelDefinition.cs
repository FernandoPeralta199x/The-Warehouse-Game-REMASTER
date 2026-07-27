using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    [CreateAssetMenu(fileName = "PuzzleLevel", menuName = "TW08/Puzzle/Level Definition")]
    public sealed class PuzzleLevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId = "prototype-001";
        [SerializeField] private string displayName = "Prototype";
        [SerializeField, Min(3)] private int width = 8;
        [SerializeField, Min(3)] private int height = 6;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private GridCoordinate playerStart = new(1, 1);
        [SerializeField] private List<GridCoordinate> walls = new();
        [SerializeField] private List<GridCoordinate> goals = new();
        [SerializeField] private List<PuzzleCrateDefinition> crates = new();
        [SerializeField, Min(0)] private int goldMoveLimit = 30;
        [SerializeField, Min(0)] private int platinumMoveLimit = 20;
        [SerializeField] private bool allowPowerUps = true;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public GridCoordinate PlayerStart => playerStart;
        public IReadOnlyList<GridCoordinate> Walls => walls;
        public IReadOnlyList<GridCoordinate> Goals => goals;
        public IReadOnlyList<PuzzleCrateDefinition> Crates => crates;
        public int GoldMoveLimit => goldMoveLimit;
        public int PlatinumMoveLimit => platinumMoveLimit;
        public bool AllowPowerUps => allowPowerUps;

        private void OnValidate()
        {
            width = Mathf.Max(3, width);
            height = Mathf.Max(3, height);
            cellSize = Mathf.Max(0.1f, cellSize);
            platinumMoveLimit = Mathf.Max(0, platinumMoveLimit);
            goldMoveLimit = Mathf.Max(platinumMoveLimit, goldMoveLimit);
        }
    }
}
