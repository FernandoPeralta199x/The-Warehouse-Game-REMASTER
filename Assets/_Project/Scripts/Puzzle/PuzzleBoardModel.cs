using System;
using System.Collections.Generic;
using System.Linq;
using TW08.Common;

namespace TW08.Puzzle
{
    public sealed class PuzzleBoardModel
    {
        private readonly HashSet<GridCoordinate> walls;
        private readonly HashSet<GridCoordinate> goals;
        private readonly Dictionary<GridCoordinate, string> crateByPosition;
        private readonly Dictionary<string, PuzzleEntityKind> crateKinds;

        public int Width { get; }
        public int Height { get; }
        public GridCoordinate PlayerPosition { get; private set; }
        public int MoveCount { get; private set; }
        public IReadOnlyCollection<GridCoordinate> Walls => walls;
        public IReadOnlyCollection<GridCoordinate> Goals => goals;
        public IReadOnlyDictionary<GridCoordinate, string> Crates => crateByPosition;
        public bool IsComplete => goals.Count > 0 && goals.Count == crateByPosition.Count && goals.All(crateByPosition.ContainsKey);

        public PuzzleBoardModel(PuzzleLevelDefinition level)
            : this(
                Guard.NotNull(level, nameof(level)).Width,
                level.Height,
                level.Walls,
                level.Goals,
                level.PlayerStart,
                level.Crates.ToDictionary(c => c.Id, c => c.Position),
                level.Crates.ToDictionary(c => c.Id, c => c.Kind))
        {
        }

        public PuzzleBoardModel(
            int width,
            int height,
            IEnumerable<GridCoordinate> walls,
            IEnumerable<GridCoordinate> goals,
            GridCoordinate playerStart,
            IReadOnlyDictionary<string, GridCoordinate> crates,
            IReadOnlyDictionary<string, PuzzleEntityKind> kinds = null)
        {
            Width = Guard.Positive(width, nameof(width));
            Height = Guard.Positive(height, nameof(height));
            this.walls = new HashSet<GridCoordinate>(walls ?? Array.Empty<GridCoordinate>());
            this.goals = new HashSet<GridCoordinate>(goals ?? Array.Empty<GridCoordinate>());
            crateByPosition = new Dictionary<GridCoordinate, string>();
            crateKinds = new Dictionary<string, PuzzleEntityKind>();
            PlayerPosition = playerStart;

            ValidateCell(playerStart, "player");

            foreach (KeyValuePair<string, GridCoordinate> crate in crates ?? new Dictionary<string, GridCoordinate>())
            {
                Guard.NotBlank(crate.Key, nameof(crates));
                ValidateCell(crate.Value, crate.Key);

                if (crateByPosition.ContainsKey(crate.Value))
                {
                    throw new InvalidOperationException($"Two crates occupy {crate.Value}.");
                }

                crateByPosition.Add(crate.Value, crate.Key);
                crateKinds[crate.Key] = kinds != null && kinds.TryGetValue(crate.Key, out PuzzleEntityKind kind)
                    ? kind
                    : PuzzleEntityKind.Crate;
            }

            if (this.walls.Contains(PlayerPosition) || crateByPosition.ContainsKey(PlayerPosition))
            {
                throw new InvalidOperationException("Player start cell is blocked.");
            }
        }

        public bool TryMove(GridCoordinate direction, out PuzzleMove move)
        {
            move = default;

            if (direction.ManhattanLength != 1)
            {
                return false;
            }

            GridCoordinate destination = PlayerPosition + direction;

            if (!IsInside(destination) || walls.Contains(destination))
            {
                return false;
            }

            GridCoordinate previousPlayer = PlayerPosition;

            if (!crateByPosition.TryGetValue(destination, out string crateId))
            {
                PlayerPosition = destination;
                MoveCount++;
                move = new PuzzleMove(previousPlayer, PlayerPosition);
                return true;
            }

            GridCoordinate crateDestination = destination + direction;

            if (!IsFree(crateDestination))
            {
                return false;
            }

            crateByPosition.Remove(destination);
            crateByPosition.Add(crateDestination, crateId);
            PlayerPosition = destination;
            MoveCount++;
            move = new PuzzleMove(previousPlayer, PlayerPosition, crateId, destination, crateDestination);
            return true;
        }

        public bool TryUndo(PuzzleMove move)
        {
            if (PlayerPosition != move.PlayerTo)
            {
                return false;
            }

            if (move.CrateMoved)
            {
                if (!crateByPosition.TryGetValue(move.CrateTo, out string crateId) || crateId != move.CrateId)
                {
                    return false;
                }

                crateByPosition.Remove(move.CrateTo);
                crateByPosition.Add(move.CrateFrom, move.CrateId);
            }

            PlayerPosition = move.PlayerFrom;
            MoveCount = Math.Max(0, MoveCount - 1);
            return true;
        }

        public bool IsInside(GridCoordinate cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
        }

        public bool IsWall(GridCoordinate cell)
        {
            return walls.Contains(cell);
        }

        public bool IsGoal(GridCoordinate cell)
        {
            return goals.Contains(cell);
        }

        public bool IsFree(GridCoordinate cell)
        {
            return IsInside(cell) && !IsWall(cell) && !crateByPosition.ContainsKey(cell);
        }

        public bool TryGetCratePosition(string crateId, out GridCoordinate position)
        {
            foreach (KeyValuePair<GridCoordinate, string> crate in crateByPosition)
            {
                if (crate.Value == crateId)
                {
                    position = crate.Key;
                    return true;
                }
            }

            position = default;
            return false;
        }

        public PuzzleEntityKind GetCrateKind(string crateId)
        {
            return crateKinds.TryGetValue(crateId, out PuzzleEntityKind kind) ? kind : PuzzleEntityKind.Crate;
        }

        private void ValidateCell(GridCoordinate cell, string label)
        {
            if (!IsInside(cell))
            {
                throw new ArgumentOutOfRangeException(label, cell, "Cell is outside the board.");
            }
        }
    }
}
