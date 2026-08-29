using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TW08.Economy;
using TW08.Puzzle;
using TW08.Save;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Cobre a economia da Oficina N-8: cálculo de Créditos de Turno, estoque de
    /// ferramentas e a migração de saves que não conheciam o sistema.
    /// </summary>
    public sealed class ShopEconomyTests
    {
        [Test]
        public void PerfectCleanRun_IsCappedAtLevelMaximum()
        {
            PuzzleRunSummary summary = new()
            {
                Moves = 20,
                Medal = 3,
                FirstAttempt = true,
                PersonalBest = true
            };

            // Somando tudo daria 425; o teto por fase segura em 250 para a loja
            // não perder sentido logo no primeiro setor.
            Assert.AreEqual(ShiftCredits.MaxPerLevel, ShiftCredits.Evaluate(summary));
        }

        [Test]
        public void PlayingPerfectlyAlwaysPaysMoreThanScrapingBy()
        {
            // Regressão: com teto único, bronze e platina numa primeira zerada
            // limpa batiam ambos em 250 — a medalha não valia nada em créditos.
            PuzzleRunSummary bronze = new()
            {
                Moves = 90, Medal = 1, FirstAttempt = true, PersonalBest = true
            };
            PuzzleRunSummary platinum = new()
            {
                Moves = 20, Medal = 3, FirstAttempt = true, PersonalBest = true
            };
            PuzzleRunSummary gold = new()
            {
                Moves = 40, Medal = 2, FirstAttempt = true, PersonalBest = true
            };

            int bronzePay = ShiftCredits.Evaluate(bronze);
            int goldPay = ShiftCredits.Evaluate(gold);
            int platinumPay = ShiftCredits.Evaluate(platinum);

            Assert.Greater(platinumPay, goldPay, "Platina precisa pagar mais que ouro.");
            Assert.Greater(goldPay, bronzePay, "Ouro precisa pagar mais que bronze.");
            Assert.LessOrEqual(platinumPay, ShiftCredits.MaxPerLevel);
        }

        [Test]
        public void BareCompletion_PaysOnlyCompletionAndBronze()
        {
            PuzzleRunSummary summary = new()
            {
                Moves = 90,
                Medal = 1,
                ToolsUsed = 1,
                HintsUsed = 2
            };

            Assert.AreEqual(
                ShiftCredits.CompletionReward + ShiftCredits.BronzeReward,
                ShiftCredits.Evaluate(summary));
        }

        [Test]
        public void UsingATool_RemovesTheCleanBonusAndClearsCompetitiveFlag()
        {
            PuzzleRunSummary clean = new() { Moves = 40, Medal = 2 };
            PuzzleRunSummary assisted = new() { Moves = 40, Medal = 2, ToolsUsed = 1 };

            Assert.IsTrue(clean.IsClean);
            Assert.IsFalse(assisted.IsClean);

            // A diferença exata depende de o turno bater ou não no teto da
            // medalha; o que precisa valer sempre é que o turno limpo paga mais.
            Assert.Greater(ShiftCredits.Evaluate(clean), ShiftCredits.Evaluate(assisted));
        }

        [Test]
        public void Statement_ListsEveryEarnedBonus()
        {
            PuzzleRunSummary summary = new() { Moves = 30, Medal = 2, PersonalBest = true };
            List<string> labels = ShiftCredits.BuildStatement(summary).Select(entry => entry.Label).ToList();

            // O extrato lista só o que foi ganho; a linha de corte é do presenter.
            CollectionAssert.AreEqual(
                new[] { "TURNO CONCLUÍDO", "MEDALHA OURO", "SEM FERRAMENTAS", "SEM DICAS", "NOVO RECORDE" },
                labels);
        }

        [Test]
        public void ToolStock_AddsAndConsumesWithoutGoingNegative()
        {
            SaveGameData data = new();
            data.AddToolCount("rewind-move", 2);
            Assert.AreEqual(2, data.GetToolCount("rewind-move"));

            data.AddToolCount("rewind-move", -5);
            Assert.AreEqual(0, data.GetToolCount("rewind-move"));
            Assert.AreEqual(0, data.GetToolCount("nunca-comprada"));
        }

        [Test]
        public void EnsureDefaults_DropsEmptyStacksAndNegativeCredits()
        {
            SaveGameData data = new() { credits = -50 };
            data.ownedTools.Add(new ToolStackRecord { toolId = "scanner", count = 0 });
            data.ownedTools.Add(new ToolStackRecord { toolId = "rewind-move", count = 3 });
            data.equippedTools.Add("   ");

            data.EnsureDefaults();

            Assert.AreEqual(0, data.credits);
            Assert.AreEqual(1, data.ownedTools.Count);
            Assert.AreEqual("rewind-move", data.ownedTools[0].toolId);
            Assert.IsEmpty(data.equippedTools);
        }

        [Test]
        public void MigrationV2ToV3_PromotesOldRecordsToCleanRanking()
        {
            // Saves v2 foram jogados antes de existir ferramenta, então o melhor
            // resultado registrado é necessariamente um turno limpo.
            SaveGameData data = new() { version = 2 };
            data.levels.Add(new LevelProgressRecord
            {
                levelId = "TW08_Level01_FirstShift",
                bestMoves = 31,
                medal = 2,
                completed = true
            });

            SaveGameData migrated = new SaveMigrationV2ToV3().Migrate(data);
            LevelProgressRecord record = migrated.levels[0];

            Assert.AreEqual(3, migrated.version);
            Assert.AreEqual(31, record.bestCleanMoves);
            Assert.AreEqual(2, record.cleanMedal);
            Assert.AreEqual(1, record.attempts);
            Assert.IsNotNull(migrated.ownedTools);
        }

        [Test]
        public void MigrationV2ToV3_LeavesUnfinishedLevelsOutOfTheCleanRanking()
        {
            SaveGameData data = new() { version = 2 };
            data.levels.Add(new LevelProgressRecord { levelId = "TW08_Level02_TightCorridor" });

            LevelProgressRecord record = new SaveMigrationV2ToV3().Migrate(data).levels[0];

            Assert.AreEqual(0, record.bestCleanMoves);
            Assert.AreEqual(0, record.cleanMedal);
        }

        [Test]
        public void Advisor_PrefersACrateLockedInACorner()
        {
            // 5x5 com paredes na borda. A carga em (1,1) está encaixada no canto
            // inferior-esquerdo; a de (2,3) está solta no meio do salão.
            PuzzleBoardModel board = BuildBoard(
                crates: new Dictionary<string, GridCoordinate>
                {
                    ["loose"] = new(2, 3),
                    ["stuck"] = new(1, 1)
                },
                goals: new[] { new GridCoordinate(3, 3), new GridCoordinate(3, 1) });

            Assert.IsTrue(PuzzleAdvisor.TryFindCriticalCrate(board, out GridCoordinate critical));
            Assert.AreEqual(new GridCoordinate(1, 1), critical);
        }

        [Test]
        public void Advisor_ReportsNoCriticalCrateWhenEverythingIsPlaced()
        {
            PuzzleBoardModel board = BuildBoard(
                crates: new Dictionary<string, GridCoordinate> { ["done"] = new(3, 3) },
                goals: new[] { new GridCoordinate(3, 3) });

            Assert.IsFalse(PuzzleAdvisor.TryFindCriticalCrate(board, out _));
            Assert.IsEmpty(PuzzleAdvisor.FindOpenGoals(board));
        }

        [Test]
        public void Advisor_HintsGetMoreDirectWithEachTier()
        {
            PuzzleBoardModel board = BuildBoard(
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 2) },
                goals: new[] { new GridCoordinate(3, 3) });

            string tier1 = PuzzleAdvisor.BuildHint(board, 1);
            string tier2 = PuzzleAdvisor.BuildHint(board, 2);
            string tier3 = PuzzleAdvisor.BuildHint(board, 3);

            Assert.IsNotEmpty(tier1);
            Assert.AreNotEqual(tier1, tier2);
            // A camada 3 aponta um passo concreto, mas nunca a solução inteira.
            StringAssert.Contains("passo", tier3.ToLowerInvariant());
        }

        private static PuzzleBoardModel BuildBoard(
            IReadOnlyDictionary<string, GridCoordinate> crates,
            IReadOnlyList<GridCoordinate> goals)
        {
            List<GridCoordinate> walls = new();
            for (int i = 0; i < 5; i++)
            {
                walls.Add(new GridCoordinate(i, 0));
                walls.Add(new GridCoordinate(i, 4));
                walls.Add(new GridCoordinate(0, i));
                walls.Add(new GridCoordinate(4, i));
            }

            return new PuzzleBoardModel(
                5, 5, walls, goals, new GridCoordinate(1, 3), crates);
        }
    }
}
