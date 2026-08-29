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
        public void RobotNoLongerBlocksCargoItCollectsIt()
        {
            // A regra mudou: o robô deixou de ser barreira para a carga. Ele
            // passa por cima e a devolve — empurrar para cima dele é permitido,
            // e é justamente o erro que a fase quer punir.
            PuzzleBoardModel board = Build(
                new[] { new GridCoordinate(1, 2), new GridCoordinate(3, 1) },
                player: new GridCoordinate(1, 1),
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _),
                "O comando é aceito; o preço o jogador paga ao ver a carga voltar.");
            Assert.AreEqual(1, board.Crates.Count, "A carga não pode sumir nem duplicar.");
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
        public void RobotReturnsCargoToWhereItStarted()
        {
            // Comando 1: o operador empurra a carga de (2,1) para (3,1) e ocupa
            // (2,1). Comando 2: ele recua, liberando a origem, e o robô chega a
            // (3,1) — recolhe a carga e a devolve para (2,1).
            PuzzleBoardModel board = Build(
                new[] { new GridCoordinate(1, 2), new GridCoordinate(2, 2), new GridCoordinate(3, 1) },
                player: new GridCoordinate(1, 1),
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(3, 1)));

            Assert.IsTrue(board.TryMove(GridCoordinate.Left, out PuzzleMove second));

            Assert.IsTrue(second.CrateReturned, "O robô precisa registrar a devolução no movimento.");
            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(2, 1)), "A carga volta para onde começou.");
            Assert.AreEqual(1, board.Crates.Count);
        }

        [Test]
        public void UndoRestoresCargoTheRobotTookAway()
        {
            // Sem a devolução registrada no movimento, desfazer restauraria o
            // operador e deixaria a carga na origem — o tabuleiro divergiria.
            PuzzleBoardModel board = Build(
                new[] { new GridCoordinate(1, 2), new GridCoordinate(2, 2), new GridCoordinate(3, 1) },
                player: new GridCoordinate(1, 1),
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) });

            board.TryMove(GridCoordinate.Right, out _);
            board.TryMove(GridCoordinate.Left, out PuzzleMove second);
            Assert.IsTrue(second.CrateReturned);

            Assert.IsTrue(board.TryUndo(second));

            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(3, 1)),
                "Desfazer devolve a carga para onde o robô a pegou.");
            Assert.AreEqual(1, board.Crates.Count, "Desfazer não pode duplicar carga.");
        }

        [Test]
        public void RobotLeavesCargoAloneWhenTheOriginIsOccupied()
        {
            // Duas cargas com a mesma origem livre indisponível: devolver
            // empilharia as duas na mesma célula e corromperia o tabuleiro.
            PuzzleBoardModel board = Build(
                new[] { new GridCoordinate(3, 1), new GridCoordinate(3, 2) },
                player: new GridCoordinate(1, 1),
                crates: new Dictionary<string, GridCoordinate>
                {
                    ["c1"] = new(2, 1),
                    ["c2"] = new(3, 1)
                });

            int before = board.Crates.Count;
            board.TryMove(GridCoordinate.Right, out _);

            Assert.AreEqual(before, board.Crates.Count, "Nenhuma carga pode sumir nem duplicar.");
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
