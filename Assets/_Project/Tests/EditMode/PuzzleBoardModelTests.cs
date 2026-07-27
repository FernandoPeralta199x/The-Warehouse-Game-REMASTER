using System.Collections.Generic;
using NUnit.Framework;
using TW08.Puzzle;

namespace TW08.Tests
{
    public sealed class PuzzleBoardModelTests
    {
        [Test]
        public void PlayerMovesIntoFreeCell()
        {
            PuzzleBoardModel board = CreateBoard();
            bool moved = board.TryMove(GridCoordinate.Up, out PuzzleMove move);
            Assert.That(moved, Is.True);
            Assert.That(board.PlayerPosition, Is.EqualTo(new GridCoordinate(1, 2)));
            Assert.That(move.CrateMoved, Is.False);
        }

        [Test]
        public void PlayerCannotMoveThroughWall()
        {
            PuzzleBoardModel board = CreateBoard(walls: new[] { new GridCoordinate(1, 2) });
            Assert.That(board.TryMove(GridCoordinate.Up, out _), Is.False);
            Assert.That(board.PlayerPosition, Is.EqualTo(new GridCoordinate(1, 1)));
        }

        [Test]
        public void PlayerPushesSingleCrate()
        {
            PuzzleBoardModel board = CreateBoard(crates: new Dictionary<string, GridCoordinate>
            {
                ["crate-a"] = new GridCoordinate(2, 1)
            });

            Assert.That(board.TryMove(GridCoordinate.Right, out PuzzleMove move), Is.True);
            Assert.That(move.CrateMoved, Is.True);
            Assert.That(board.TryGetCratePosition("crate-a", out GridCoordinate position), Is.True);
            Assert.That(position, Is.EqualTo(new GridCoordinate(3, 1)));
        }

        [Test]
        public void PlayerCannotPushTwoCrates()
        {
            PuzzleBoardModel board = CreateBoard(crates: new Dictionary<string, GridCoordinate>
            {
                ["crate-a"] = new GridCoordinate(2, 1),
                ["crate-b"] = new GridCoordinate(3, 1)
            });

            Assert.That(board.TryMove(GridCoordinate.Right, out _), Is.False);
        }

        [Test]
        public void UndoRestoresPlayerAndCrate()
        {
            PuzzleBoardModel board = CreateBoard(crates: new Dictionary<string, GridCoordinate>
            {
                ["crate-a"] = new GridCoordinate(2, 1)
            });
            board.TryMove(GridCoordinate.Right, out PuzzleMove move);

            Assert.That(board.TryUndo(move), Is.True);
            Assert.That(board.PlayerPosition, Is.EqualTo(new GridCoordinate(1, 1)));
            Assert.That(board.TryGetCratePosition("crate-a", out GridCoordinate position), Is.True);
            Assert.That(position, Is.EqualTo(new GridCoordinate(2, 1)));
            Assert.That(board.MoveCount, Is.EqualTo(0));
        }

        [Test]
        public void BoardCompletesWhenEveryGoalHasCrate()
        {
            PuzzleBoardModel board = CreateBoard(
                goals: new[] { new GridCoordinate(3, 1) },
                crates: new Dictionary<string, GridCoordinate> { ["crate-a"] = new GridCoordinate(2, 1) });
            board.TryMove(GridCoordinate.Right, out _);
            Assert.That(board.IsComplete, Is.True);
        }

        private static PuzzleBoardModel CreateBoard(
            IEnumerable<GridCoordinate> walls = null,
            IEnumerable<GridCoordinate> goals = null,
            IReadOnlyDictionary<string, GridCoordinate> crates = null)
        {
            return new PuzzleBoardModel(
                6,
                6,
                walls ?? new GridCoordinate[0],
                goals ?? new GridCoordinate[0],
                new GridCoordinate(1, 1),
                crates ?? new Dictionary<string, GridCoordinate>());
        }
    }
}
