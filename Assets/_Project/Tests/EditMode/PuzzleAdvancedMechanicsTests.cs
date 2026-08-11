using System.Collections.Generic;
using NUnit.Framework;
using TW08.Puzzle;

namespace TW08.Tests.EditMode
{
    public sealed class PuzzleAdvancedMechanicsTests
    {
        [Test]
        public void CostlyCellAddsTwoMovesAndUndoRestoresScore()
        {
            PuzzleBoardModel board = new(
                5, 5,
                new GridCoordinate[0],
                new[] { new GridCoordinate(4, 4) },
                new GridCoordinate(1, 1),
                new Dictionary<string, GridCoordinate> { { "crate", new GridCoordinate(3, 3) } },
                null,
                new[] { new GridCoordinate(2, 1) });

            Assert.That(board.TryMove(GridCoordinate.Right, out PuzzleMove move), Is.True);
            Assert.That(move.MoveCost, Is.EqualTo(2));
            Assert.That(board.MoveCount, Is.EqualTo(2));
            Assert.That(board.TryUndo(move), Is.True);
            Assert.That(board.MoveCount, Is.Zero);
            Assert.That(board.PlayerPosition, Is.EqualTo(new GridCoordinate(1, 1)));
        }

        [Test]
        public void DynamicDoorBlocksAndUnblocksMovement()
        {
            PuzzleBoardModel board = new(
                5, 5,
                new GridCoordinate[0],
                new[] { new GridCoordinate(4, 4) },
                new GridCoordinate(1, 1),
                new Dictionary<string, GridCoordinate> { { "crate", new GridCoordinate(3, 3) } });

            GridCoordinate door = new(2, 1);
            Assert.That(board.SetDynamicBlocked(door, true), Is.True);
            Assert.That(board.TryMove(GridCoordinate.Right, out _), Is.False);
            Assert.That(board.SetDynamicBlocked(door, false), Is.True);
            Assert.That(board.TryMove(GridCoordinate.Right, out _), Is.True);
        }

        [Test]
        public void TypedDockRejectsWrongCargoKind()
        {
            GridCoordinate goal = new(3, 1);
            Dictionary<string, GridCoordinate> crates = new() { { "cargo", new GridCoordinate(2, 1) } };
            Dictionary<string, PuzzleEntityKind> kinds = new() { { "cargo", PuzzleEntityKind.HeavyCrate } };
            Dictionary<GridCoordinate, PuzzleEntityKind> requirements = new() { { goal, PuzzleEntityKind.FragileCrate } };

            PuzzleBoardModel board = new(
                5, 4, new GridCoordinate[0], new[] { goal }, new GridCoordinate(1, 1),
                crates, kinds, null, requirements);

            Assert.That(board.TryMove(GridCoordinate.Right, out _), Is.True);
            Assert.That(board.IsComplete, Is.False);
        }

        [Test]
        public void TypedDockAcceptsMatchingCargoKind()
        {
            GridCoordinate goal = new(3, 1);
            Dictionary<string, GridCoordinate> crates = new() { { "cargo", new GridCoordinate(2, 1) } };
            Dictionary<string, PuzzleEntityKind> kinds = new() { { "cargo", PuzzleEntityKind.HeavyCrate } };
            Dictionary<GridCoordinate, PuzzleEntityKind> requirements = new() { { goal, PuzzleEntityKind.HeavyCrate } };

            PuzzleBoardModel board = new(
                5, 4, new GridCoordinate[0], new[] { goal }, new GridCoordinate(1, 1),
                crates, kinds, null, requirements);

            Assert.That(board.TryMove(GridCoordinate.Right, out _), Is.True);
            Assert.That(board.IsComplete, Is.True);
        }
    }
}
