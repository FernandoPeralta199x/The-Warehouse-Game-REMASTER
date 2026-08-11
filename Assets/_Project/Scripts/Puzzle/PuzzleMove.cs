using System;

namespace TW08.Puzzle
{
    public readonly struct PuzzleMove
    {
        public GridCoordinate PlayerFrom { get; }
        public GridCoordinate PlayerTo { get; }
        public bool CrateMoved { get; }
        public string CrateId { get; }
        public GridCoordinate CrateFrom { get; }
        public GridCoordinate CrateTo { get; }
        public int MoveCost { get; }

        public PuzzleMove(GridCoordinate playerFrom, GridCoordinate playerTo, int moveCost = 1)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            CrateMoved = false;
            CrateId = string.Empty;
            CrateFrom = default;
            CrateTo = default;
            MoveCost = Math.Max(1, moveCost);
        }

        public PuzzleMove(
            GridCoordinate playerFrom,
            GridCoordinate playerTo,
            string crateId,
            GridCoordinate crateFrom,
            GridCoordinate crateTo,
            int moveCost = 1)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            CrateMoved = true;
            CrateId = crateId;
            CrateFrom = crateFrom;
            CrateTo = crateTo;
            MoveCost = Math.Max(1, moveCost);
        }
    }
}
