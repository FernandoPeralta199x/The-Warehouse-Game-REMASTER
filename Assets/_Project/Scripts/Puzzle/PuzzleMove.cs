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

        /// <summary>
        /// Carga que o robô de limpeza recolheu neste comando, se houve.
        ///
        /// Precisa viver no movimento para o desfazer conseguir devolvê-la ao
        /// lugar de onde o robô a tirou: sem isso, desfazer restauraria o
        /// jogador mas deixaria a carga na origem, e o tabuleiro divergiria.
        /// </summary>
        public string ReturnedCrateId { get; }
        public GridCoordinate ReturnedFrom { get; }
        public GridCoordinate ReturnedTo { get; }
        public bool CrateReturned => !string.IsNullOrEmpty(ReturnedCrateId);

        /// <summary>Cópia deste movimento acrescida da devolução do robô.</summary>
        public PuzzleMove WithReturn(string crateId, GridCoordinate from, GridCoordinate to)
        {
            return new PuzzleMove(this, crateId, from, to);
        }

        private PuzzleMove(PuzzleMove source, string returnedCrateId, GridCoordinate from, GridCoordinate to)
        {
            PlayerFrom = source.PlayerFrom;
            PlayerTo = source.PlayerTo;
            Direction = source.Direction;
            CrateMoved = source.CrateMoved;
            CrateId = source.CrateId;
            CrateFrom = source.CrateFrom;
            CrateTo = source.CrateTo;
            MoveCost = source.MoveCost;
            ReturnedCrateId = returnedCrateId;
            ReturnedFrom = from;
            ReturnedTo = to;
        }

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
            ReturnedCrateId = null;
            ReturnedFrom = default;
            ReturnedTo = default;
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
            ReturnedCrateId = null;
            ReturnedFrom = default;
            ReturnedTo = default;
        }
    }
}
