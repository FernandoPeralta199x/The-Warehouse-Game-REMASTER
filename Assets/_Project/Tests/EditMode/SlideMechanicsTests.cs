using System.Collections.Generic;
using NUnit.Framework;
using TW08.Puzzle;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Deslizamento em gelo e esteira.
    ///
    /// Estes testes são o contrato que o solver Python espelha: se a semântica
    /// mudar aqui sem mudar lá, as 27 fases deixam de estar provadas.
    /// </summary>
    public sealed class SlideMechanicsTests
    {
        // Corredor 8x3 com bordas de parede; a faixa central é o que varia.
        private static PuzzleBoardModel Build(
            IEnumerable<GridCoordinate> ice = null,
            IReadOnlyDictionary<GridCoordinate, GridCoordinate> conveyors = null,
            IReadOnlyDictionary<string, GridCoordinate> crates = null,
            GridCoordinate? player = null,
            int width = 8)
        {
            List<GridCoordinate> walls = new();
            for (int x = 0; x < width; x++)
            {
                walls.Add(new GridCoordinate(x, 0));
                walls.Add(new GridCoordinate(x, 2));
            }

            walls.Add(new GridCoordinate(0, 1));
            walls.Add(new GridCoordinate(width - 1, 1));

            return new PuzzleBoardModel(
                width, 3, walls,
                new[] { new GridCoordinate(width - 2, 1) },
                player ?? new GridCoordinate(1, 1),
                crates ?? new Dictionary<string, GridCoordinate>(),
                null, null, null,
                ice, conveyors);
        }

        // ------------------------------------------------------------ Gelo --

        [Test]
        public void PlayerKeepsSlidingUntilTheIceEnds()
        {
            // Gelo em 2..4; o jogador entra em 2 e só para ao pisar em 5.
            PuzzleBoardModel board = Build(ice: new[]
            {
                new GridCoordinate(2, 1), new GridCoordinate(3, 1), new GridCoordinate(4, 1)
            });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.AreEqual(new GridCoordinate(5, 1), board.PlayerPosition);
            Assert.AreEqual(new GridCoordinate(5, 1), move.PlayerTo);
        }

        [Test]
        public void SlidingCostsOneCommandNotOneCellPerStep()
        {
            PuzzleBoardModel board = Build(ice: new[]
            {
                new GridCoordinate(2, 1), new GridCoordinate(3, 1), new GridCoordinate(4, 1)
            });

            board.TryMove(GridCoordinate.Right, out _);

            // Deslizar é um gesto só. Cobrar por célula tornaria o gelo um
            // castigo, quando o desenho das fases o usa como atalho.
            Assert.AreEqual(1, board.MoveCount);
        }

        [Test]
        public void SlidingStopsAgainstAWall()
        {
            // Gelo até a borda: o jogador para na última célula livre.
            PuzzleBoardModel board = Build(ice: new[]
            {
                new GridCoordinate(2, 1), new GridCoordinate(3, 1),
                new GridCoordinate(4, 1), new GridCoordinate(5, 1), new GridCoordinate(6, 1)
            });

            board.TryMove(GridCoordinate.Right, out _);
            Assert.AreEqual(new GridCoordinate(6, 1), board.PlayerPosition);
        }

        [Test]
        public void PushedCrateSlidesAndThePlayerStopsBehindIt()
        {
            // Carga em 3, que é piso comum, com gelo à frente em 4..5. Empurrada,
            // ela entra no gelo e corre até 6; o jogador ocupa 3 e para ali,
            // porque quem não pisa no gelo não desliza.
            PuzzleBoardModel board = Build(
                ice: new[] { new GridCoordinate(4, 1), new GridCoordinate(5, 1) },
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(3, 1) },
                player: new GridCoordinate(2, 1));

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.AreEqual(new GridCoordinate(6, 1), move.CrateTo);
            Assert.AreEqual(new GridCoordinate(3, 1), board.PlayerPosition);
        }

        [Test]
        public void PlayerNeverSlidesThroughTheCrateItJustPushed()
        {
            // Jogador e carga ambos sobre gelo: a carga para na parede em 6 e o
            // jogador precisa parar em 5, encostado nela — nunca atravessá-la.
            PuzzleBoardModel board = Build(
                ice: new[]
                {
                    new GridCoordinate(2, 1), new GridCoordinate(3, 1), new GridCoordinate(4, 1),
                    new GridCoordinate(5, 1), new GridCoordinate(6, 1)
                },
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) },
                player: new GridCoordinate(1, 1));

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.AreEqual(new GridCoordinate(6, 1), move.CrateTo);
            Assert.AreEqual(new GridCoordinate(5, 1), board.PlayerPosition);
        }

        [Test]
        public void UndoRestoresBothEndsOfASlide()
        {
            PuzzleBoardModel board = Build(
                ice: new[] { new GridCoordinate(3, 1), new GridCoordinate(4, 1), new GridCoordinate(5, 1) },
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(3, 1) },
                player: new GridCoordinate(2, 1));

            board.TryMove(GridCoordinate.Right, out PuzzleMove move);
            Assert.IsTrue(board.TryUndo(move));

            Assert.AreEqual(new GridCoordinate(2, 1), board.PlayerPosition);
            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(3, 1)));
            Assert.AreEqual(0, board.MoveCount);
        }

        // --------------------------------------------------------- Esteira --

        [Test]
        public void ConveyorImposesItsOwnDirection()
        {
            // Esteira em 3 apontando para a esquerda: quem entra indo à direita
            // é levado de volta. É o que torna a correia um obstáculo e não um atalho.
            PuzzleBoardModel board = Build(conveyors: new Dictionary<GridCoordinate, GridCoordinate>
            {
                [new GridCoordinate(3, 1)] = GridCoordinate.Left
            }, player: new GridCoordinate(2, 1));

            board.TryMove(GridCoordinate.Right, out _);
            Assert.AreEqual(new GridCoordinate(2, 1), board.PlayerPosition);
        }

        [Test]
        public void ConveyorCarriesACrateToTheEndOfTheBelt()
        {
            PuzzleBoardModel board = Build(
                conveyors: new Dictionary<GridCoordinate, GridCoordinate>
                {
                    [new GridCoordinate(3, 1)] = GridCoordinate.Right,
                    [new GridCoordinate(4, 1)] = GridCoordinate.Right,
                    [new GridCoordinate(5, 1)] = GridCoordinate.Right
                },
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(3, 1) },
                player: new GridCoordinate(2, 1));

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.AreEqual(new GridCoordinate(6, 1), move.CrateTo);
        }

        [Test]
        public void ClosedConveyorLoopTerminatesInsteadOfHanging()
        {
            // Anel de esteiras 1..2 x 1..2 num tabuleiro 4x4. Sem o teto de
            // iterações do Slide, entrar aqui giraria para sempre.
            List<GridCoordinate> walls = new();
            for (int i = 0; i < 4; i++)
            {
                walls.Add(new GridCoordinate(i, 0));
                walls.Add(new GridCoordinate(i, 3));
                walls.Add(new GridCoordinate(0, i));
                walls.Add(new GridCoordinate(3, i));
            }

            Dictionary<GridCoordinate, GridCoordinate> ring = new()
            {
                [new GridCoordinate(1, 1)] = GridCoordinate.Right,
                [new GridCoordinate(2, 1)] = GridCoordinate.Up,
                [new GridCoordinate(2, 2)] = GridCoordinate.Left,
                [new GridCoordinate(1, 2)] = GridCoordinate.Down
            };

            PuzzleBoardModel board = new(
                4, 4, walls,
                new[] { new GridCoordinate(2, 2) },
                new GridCoordinate(1, 1),
                new Dictionary<string, GridCoordinate>(),
                null, null, null, null, ring);

            Assert.DoesNotThrow(() => board.TryMove(GridCoordinate.Right, out _));
        }

        [Test]
        public void PlainFloorStillMovesExactlyOneCell()
        {
            // Sem gelo nem esteira nada muda: é a garantia de que as fases sem
            // gimmick continuam com a mesma semântica de sempre.
            PuzzleBoardModel board = Build();

            board.TryMove(GridCoordinate.Right, out _);
            Assert.AreEqual(new GridCoordinate(2, 1), board.PlayerPosition);
            Assert.AreEqual(1, board.MoveCount);
        }
    }
}
