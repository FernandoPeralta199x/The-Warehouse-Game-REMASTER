using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    [CreateAssetMenu(fileName = "PuzzleLevel", menuName = "TW08/Puzzle/Level Definition")]
    public sealed class PuzzleLevelDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string levelId = "prototype-001";
        [SerializeField] private string displayName = "Prototype";
        [SerializeField] private string sectorId = "S01";
        [SerializeField, TextArea(2, 5)] private string briefing = string.Empty;
        [SerializeField] private Sprite previewImage;
        [SerializeField] private List<string> gimmickTags = new();

        [Header("Board")]
        [SerializeField, Min(3)] private int width = 8;
        [SerializeField, Min(3)] private int height = 6;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private GridCoordinate playerStart = new(1, 1);
        [SerializeField] private List<GridCoordinate> walls = new();
        [SerializeField] private List<GridCoordinate> goals = new();
        [SerializeField] private List<PuzzleGoalRequirementDefinition> goalRequirements = new();
        [SerializeField] private List<PuzzleCrateDefinition> crates = new();

        [Header("Mechanics")]
        [Tooltip("Moving into one of these cells costs two moves instead of one. Used by cold-storage floors without introducing non-deterministic physics.")]
        [SerializeField] private List<GridCoordinate> costlyCells = new();
        [Tooltip("Each group opens its doors while every sensor in the group is occupied by a crate.")]
        [SerializeField] private List<PuzzleSwitchGroupDefinition> switchGroups = new();
        [Tooltip("Ice: whoever enters keeps sliding in the same direction until leaving the ice or hitting something.")]
        [SerializeField] private List<GridCoordinate> iceCells = new();
        [Tooltip("Conveyors carry whoever enters them along their own fixed direction.")]
        [SerializeField] private List<PuzzleConveyorDefinition> conveyors = new();
        [Tooltip("Free cells drawn as walls until the player gets close. Presentation only: the board always treats them as open.")]
        [SerializeField] private List<GridCoordinate> fakeWalls = new();
        [Tooltip("Fog of war mode. Hides information without changing what is solvable.")]
        [SerializeField] private PuzzleFogMode fogMode = PuzzleFogMode.None;
        [SerializeField, Min(1)] private int fogRadius = 2;
        [Tooltip("Cleaning robots. Each advances one step along its route per player command.")]
        [SerializeField] private List<PuzzlePatrolDefinition> patrols = new();
        [Tooltip("Stepping on one of these flips every conveyor on the board.")]
        [SerializeField] private List<GridCoordinate> directionButtons = new();
        [Tooltip("Cells blocked until the shift reaches a given command count.")]
        [SerializeField] private List<PuzzleTimedBlockDefinition> timedBlocks = new();

        [Header("Scoring")]
        [SerializeField, Min(0)] private int goldMoveLimit = 30;
        [SerializeField, Min(0)] private int platinumMoveLimit = 20;
        [SerializeField] private bool allowPowerUps = true;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public string SectorId => sectorId;
        public string Briefing => briefing;
        public Sprite PreviewImage => previewImage;
        public IReadOnlyList<string> GimmickTags => gimmickTags;
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public GridCoordinate PlayerStart => playerStart;
        public IReadOnlyList<GridCoordinate> Walls => walls;
        public IReadOnlyList<GridCoordinate> Goals => goals;
        public IReadOnlyList<PuzzleGoalRequirementDefinition> GoalRequirements => goalRequirements;
        public IReadOnlyList<PuzzleCrateDefinition> Crates => crates;
        public IReadOnlyList<GridCoordinate> CostlyCells => costlyCells;
        public IReadOnlyList<PuzzleSwitchGroupDefinition> SwitchGroups => switchGroups;
        public IReadOnlyList<GridCoordinate> IceCells => iceCells;
        public IReadOnlyList<PuzzleConveyorDefinition> Conveyors => conveyors;
        public IReadOnlyList<GridCoordinate> FakeWalls => fakeWalls;
        public PuzzleFogMode FogMode => fogMode;
        public int FogRadius => Mathf.Max(1, fogRadius);
        public IReadOnlyList<PuzzlePatrolDefinition> Patrols => patrols;
        public IReadOnlyList<GridCoordinate> DirectionButtons => directionButtons;
        public IReadOnlyList<PuzzleTimedBlockDefinition> TimedBlocks => timedBlocks;
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
            sectorId = string.IsNullOrWhiteSpace(sectorId) ? "S01" : sectorId.Trim();
        }
    }
}
