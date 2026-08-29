using System;

namespace TW08.Puzzle
{
    public readonly struct PuzzleMove
    {
        public GridCoordinate PlayerFrom { get; }
        public GridCoordinate PlayerTo { get; }

        /// <summary>
        /// Direção do comando, sempre unitária.
        ///
        /// Não dá para deduzi-la de PlayerTo - PlayerFrom: com gelo ou esteira o
        /// operador percorre várias células num comando só, e a diferença vira
        /// um vetor de comprimento maior que 1 — que TryMove recusa.
        /// </summary>
        public GridCoordinate Direction { get; }
        public bool CrateMoved { get; }
        public string CrateId { get; }
        public GridCoordinate CrateFrom { get; }
        public GridCoordinate CrateTo { get; }
        public int MoveCost { get; }

        public PuzzleMove(
            GridCoordinate playerFrom,
            GridCoordinate playerTo,
            int moveCost = 1,
            GridCoordinate direction = default)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            Direction = direction.ManhattanLength == 1 ? direction : playerTo - playerFrom;
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
            int moveCost = 1,
            GridCoordinate direction = default)
        {
            PlayerFrom = playerFrom;
            PlayerTo = playerTo;
            Direction = direction.ManhattanLength == 1 ? direction : playerTo - playerFrom;
            CrateMoved = true;
            CrateId = crateId;
            CrateFrom = crateFrom;
            CrateTo = crateTo;
            MoveCost = Math.Max(1, moveCost);
        }
    }
}
