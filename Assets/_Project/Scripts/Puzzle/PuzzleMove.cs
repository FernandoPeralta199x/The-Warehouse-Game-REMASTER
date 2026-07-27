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

        public PuzzleMove(GridCoordinate playerFrom, GridCoordinate playerTo)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            CrateMoved = false;
            CrateId = string.Empty;
            CrateFrom = default;
            CrateTo = default;
        }

        public PuzzleMove(
            GridCoordinate playerFrom,
            GridCoordinate playerTo,
            string crateId,
            GridCoordinate crateFrom,
            GridCoordinate crateTo)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            CrateMoved = true;
            CrateId = crateId;
            CrateFrom = crateFrom;
            CrateTo = crateTo;
        }
    }
}
