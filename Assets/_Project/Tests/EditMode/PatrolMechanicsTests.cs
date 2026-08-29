using System.Collections.Generic;
using NUnit.Framework;
using TW08.Puzzle;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Robô de limpeza. A posição dele é função do número de comandos, e é isso
    /// que permite ao solver provar a fase sem simular o robô em paralelo.
    /// </summary>
    public sealed class PatrolMechanicsTests
    {
        private static PuzzleBoardModel Build(
            IEnumerable<GridCoordinate> route,
            GridCoordinate? player = null,
            IReadOnlyDictionary<string, GridCoordinate> crates = null)
        {
            List<GridCoordinate> walls = new();
            for (int x = 0; x < 6; x++)
            {
                walls.Add(new GridCoordinate(x, 0));
                walls.Add(new GridCoordinate(x, 3));
            }

            for (int y = 0; y < 4; y++)
            {
                walls.Add(new GridCoordinate(0, y));
                walls.Add(new GridCoordinate(5, y));
            }

            List<PuzzlePatrolDefinition> patrols = new();
            if (route != null)
            {
                patrols.Add(new PuzzlePatrolDefinition("bot", route));
            }

            return new PuzzleBoardModel(
                6, 4, walls,
                new[] { new GridCoordinate(4, 1) },
                player ?? new GridCoordinate(1, 1),
                crates ?? new Dictionary<string, GridCoordinate>(),
                null, null, null, null, null, patrols);
        }

        [Test]
        public void RouteWrapsAroundAndRepeats()
        {
            PuzzlePatrolDefinition patrol = new("bot", new[]
            {
                new GridCoordinate(1, 2), new GridCoordinate(2, 2), new GridCoordinate(3, 2)
            });

            Assert.AreEqual(new GridCoordinate(1, 2), patrol.PositionAt(0));
            Assert.AreEqual(new GridCoordinate(3, 2), patrol.PositionAt(2));
            Assert.AreEqual(new GridCoordinate(1, 2), patrol.PositionAt(3), "A rota é um ciclo.");
        }

        [Test]
        public void NegativeStepStillLandsOnTheRoute()
        {
            // Passo negativo acontece ao desfazer no primeiro comando da fase.
            PuzzlePatrolDefinition patrol = new("bot", new[]
            {
                new GridCoordinate(1, 2), new GridCoordinate(2, 2), new GridCoordinate(3, 2)
            });

            Assert.AreEqual(new GridCoordinate(3, 2), patrol.PositionAt(-1));
        }

        [Test]
        public void MoveIsRefusedWhenItWouldEndInsideTheRobot()
        {
            // O robô estará em (2,1) depois deste comando, que é exatamente onde
            // o jogador pretende parar.
            PuzzleBoardModel board = Build(new[]
            {
                new GridCoordinate(1, 2), new GridCoordinate(2, 1)
            });

            Assert.IsFalse(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(1, 1), board.PlayerPosition, "Movimento recusado não mexe o jogador.");
            Assert.AreEqual(0, board.CommandCount, "Movimento recusado não adianta o relógio.");
        }

        [Test]
        public void SameCellIsFreeOnceTheRobotHasMovedOn()
        {
            // O robô só chega a (2,1) no comando 2, então o comando 1 passa.
            PuzzleBoardModel board = Build(new[]
            {
                new GridCoordinate(3, 2), new GridCoordinate(2, 2), new GridCoordinate(2, 1)
            });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(2, 1), board.PlayerPosition);
        }

        [Test]
        public void RobotBlocksACrateFromBeingPushedIntoIt()
        {
            PuzzleBoardModel board = Build(
                new[] { new GridCoordinate(1, 2), new GridCoordinate(3, 1) },
                player: new GridCoordinate(1, 1),
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) });

            // A carga iria para (3,1), onde o robô estará.
            Assert.IsFalse(board.TryMove(GridCoordinate.Right, out _));
            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(2, 1)), "A carga não pode ter se mexido.");
        }

        [Test]
        public void CommandClockDrivesTheRobotAndUndoRewindsIt()
        {
            PuzzleBoardModel board = Build(new[]
            {
                new GridCoordinate(1, 2), new GridCoordinate(2, 2), new GridCoordinate(3, 2)
            });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.AreEqual(1, board.CommandCount);
            Assert.AreEqual(new GridCoordinate(2, 2), board.Patrols[0].PositionAt(board.CommandCount));

            board.TryUndo(move);
            Assert.AreEqual(0, board.CommandCount, "Desfazer devolve o robô ao passo anterior.");
            Assert.AreEqual(new GridCoordinate(1, 2), board.Patrols[0].PositionAt(board.CommandCount));
        }

        [Test]
        public void BoardWithoutPatrolsKeepsTheOldBehaviour()
        {
            PuzzleBoardModel board = Build(route: null);

            Assert.IsEmpty(board.Patrols);
            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(2, 1), board.PlayerPosition);
        }
    }
}
