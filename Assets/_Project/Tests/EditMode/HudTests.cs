using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TW08.Economy;
using TW08.Puzzle;
using TW08.UI.Hud;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Cobre a parte do HUD que dá para julgar sem montar cena: formatação de
    /// rótulos, decisão da faixa de status e a montagem do extrato de fim de
    /// turno. A animação em si é decorativa — o que não pode errar é o texto e o
    /// número que o jogador lê.
    /// </summary>
    public sealed class HudTests
    {
        // ----------------------------------------------------- Formatação --

        [Test]
        public void MoveSummary_KeepsTheLegacyCompositeLayout()
        {
            // Cenas antigas ainda têm um rótulo único; o formato não pode mudar.
            Assert.AreEqual(
                "MOVIMENTOS 007   UNDO 03   REDO 01",
                HudFormat.MoveSummary(7, 3, 1));
        }

        [Test]
        public void MoveHistory_LeavesTheNumberForTheAnimatedCounter()
        {
            Assert.AreEqual("UNDO 12   REDO 00", HudFormat.MoveHistory(12, 0));
        }

        [Test]
        public void MovesValueFormat_IsUsableByTheCounterTween()
        {
            // CountTo aplica este formato a cada frame: precisa aceitar um único
            // argumento inteiro e continuar com o mesmo preenchimento.
            Assert.AreEqual("MOVIMENTOS 007", string.Format(HudFormat.MovesValueFormat, 7));
            Assert.AreEqual("MOVIMENTOS 128", string.Format(HudFormat.MovesValueFormat, 128));
        }

        [Test]
        public void NegativeCounters_NeverReachTheScreen()
        {
            Assert.AreEqual("MOVIMENTOS 000   UNDO 00   REDO 00", HudFormat.MoveSummary(-4, -2, -9));
            Assert.AreEqual("PLAT 000 // GOLD 000", HudFormat.Targets(-1, -1));
        }

        [Test]
        public void LevelTitle_JoinsSectorAndNameInUppercase()
        {
            Assert.AreEqual("S02 // CORREDOR APERTADO", HudFormat.LevelTitle("s02", "Corredor Apertado"));
        }

        [Test]
        public void LevelTitle_FallsBackWhenTheLevelHasNoIdentity()
        {
            Assert.AreEqual("S-- // ROTA SEM NOME", HudFormat.LevelTitle("  ", null));
        }

        [Test]
        public void Operator_AlwaysHasAPlaceholder()
        {
            Assert.AreEqual("OPERADOR // DUDA", HudFormat.Operator("duda"));
            Assert.AreEqual("OPERADOR // --", HudFormat.Operator(null));
        }

        [Test]
        public void Time_UsesInvariantSeparatorsAndPadding()
        {
            // O ponto decimal é fixo: uma localidade com vírgula quebraria a
            // leitura do cronômetro e a comparação visual com o recorde.
            Assert.AreEqual("00:00.000", HudFormat.Time(0f));
            Assert.AreEqual("01:05.250", HudFormat.Time(65.25f));
            Assert.AreEqual("00:00.000", HudFormat.Time(-12f));
        }

        [Test]
        public void BestTime_ShowsAPlaceholderUntilThereIsARecord()
        {
            Assert.AreEqual("BEST --:--.---", HudFormat.BestTime(0f));
            Assert.AreEqual("BEST 00:42.500", HudFormat.BestTime(42.5f));
        }

        [Test]
        public void Lap_ClampsToTheTrackLength()
        {
            Assert.AreEqual("VOLTA 02/03", HudFormat.Lap(2, 3));
            // Ao cruzar a linha final o progresso já aponta a volta seguinte.
            Assert.AreEqual("VOLTA 03/03", HudFormat.Lap(4, 3));
            Assert.AreEqual("VOLTA 01/01", HudFormat.Lap(0, 0));
        }

        [Test]
        public void Position_ShowsAPlaceholderBeforeTheFirstStanding()
        {
            Assert.AreEqual("POS 02/06", HudFormat.Position(2, 6));
            Assert.AreEqual("POS --/01", HudFormat.Position(0, 0));
        }

        [Test]
        public void CargoIntegrity_ReportsLossBeforePercentage()
        {
            Assert.AreEqual("CARGA // PERDIDA", HudFormat.CargoIntegrity(0.8f, true));
            Assert.AreEqual("CARGA // 080%", HudFormat.CargoIntegrity(0.8f, false));
            Assert.AreEqual("CARGA // 100%", HudFormat.CargoIntegrity(4f, false));
        }

        [Test]
        public void SpeedReading_StaysNonNegativeForTheCounter()
        {
            Assert.AreEqual(0, HudFormat.SpeedReading(-3f));
            Assert.Greater(HudFormat.SpeedReading(6f), HudFormat.SpeedReading(3f));
        }

        [Test]
        public void Signed_AlwaysShowsTheSignOnTheStatement()
        {
            Assert.AreEqual("+50", HudFormat.Signed(50));
            Assert.AreEqual("-175", HudFormat.Signed(-175));
            Assert.AreEqual("+0", HudFormat.Signed(0));
        }

        [Test]
        public void RankingCopy_DistinguishesCleanFromAssisted()
        {
            Assert.AreEqual("TURNO LIMPO", HudFormat.RankingChip(false));
            Assert.AreEqual("TURNO ASSISTIDO", HudFormat.RankingChip(true));
            Assert.AreEqual("TURNO LIMPO // RANKING ATIVO", HudFormat.RankingLine(false));
            Assert.AreEqual("MODO ASSISTIDO // FORA DO RANKING", HudFormat.RankingLine(true));
        }

        [Test]
        public void DoorNotice_NamesTheGroupInUppercase()
        {
            Assert.AreEqual("PORTA G1 ABERTA", HudFormat.DoorNotice("g1", true));
            Assert.AreEqual("PORTA -- FECHADA", HudFormat.DoorNotice(null, false));
        }

        // --------------------------------------------------------- Status --

        [Test]
        public void Status_CompletionWinsOverDeadlock()
        {
            // Encaixar a última carga num canto que também é alvo conclui a fase:
            // seria absurdo alarmar travamento no frame da vitória.
            Assert.AreEqual(
                PuzzleHudStatus.Complete,
                PuzzleHudStatusResolver.Resolve(hasBoard: true, isComplete: true, deadlocked: true));
        }

        [Test]
        public void Status_WithoutABoardIsUnavailable()
        {
            Assert.AreEqual(
                PuzzleHudStatus.Unavailable,
                PuzzleHudStatusResolver.Resolve(hasBoard: false, isComplete: false, deadlocked: false));
        }

        [Test]
        public void Status_LabelsMatchTheTerminalCopy()
        {
            Assert.AreEqual("ROTA ATIVA", PuzzleHudStatusResolver.LabelFor(PuzzleHudStatus.Active));
            Assert.AreEqual(
                "ALERTA: CARGA TRAVADA // USE UNDO",
                PuzzleHudStatusResolver.LabelFor(PuzzleHudStatus.Deadlock));
            Assert.AreEqual("ROTA INDISPONÍVEL", PuzzleHudStatusResolver.LabelFor(PuzzleHudStatus.Unavailable));
        }

        [Test]
        public void Status_OnlyDeadlockTriggersTheAlarm()
        {
            Assert.IsTrue(PuzzleHudStatusResolver.IsAlarming(PuzzleHudStatus.Deadlock));
            Assert.IsFalse(PuzzleHudStatusResolver.IsAlarming(PuzzleHudStatus.Active));
            Assert.IsFalse(PuzzleHudStatusResolver.IsAlarming(PuzzleHudStatus.Complete));
        }

        [Test]
        public void CompletionLabel_ShowsCreditsOnlyAfterTheShiftIsCommitted()
        {
            Assert.AreEqual(
                "ROTA LIBERADA // MEDALHA 2 // LIMPO",
                PuzzleHudStatusResolver.CompletionLabel(2, assisted: false, creditsEarned: 180, hasReport: false));

            Assert.AreEqual(
                "ROTA LIBERADA // MEDALHA 2 // ASSISTIDO // +180 CRÉDITOS",
                PuzzleHudStatusResolver.CompletionLabel(2, assisted: true, creditsEarned: 180, hasReport: true));
        }

        [Test]
        public void Status_ColorsAreDistinctPerState()
        {
            Assert.AreEqual(HudPalette.Red, PuzzleHudStatusResolver.ColorFor(PuzzleHudStatus.Deadlock));
            Assert.AreEqual(HudPalette.Green, PuzzleHudStatusResolver.ColorFor(PuzzleHudStatus.Active));
            Assert.AreNotEqual(
                PuzzleHudStatusResolver.ColorFor(PuzzleHudStatus.Complete),
                PuzzleHudStatusResolver.ColorFor(PuzzleHudStatus.Active));
        }

        [Test]
        public void Status_MatchesTheRealDeadlockDetectorOnACorneredCrate()
        {
            PuzzleBoardModel board = BuildBoard(
                crates: new Dictionary<string, GridCoordinate> { ["stuck"] = new(1, 1) },
                goals: new[] { new GridCoordinate(3, 3) });

            PuzzleHudStatus status = PuzzleHudStatusResolver.Resolve(
                hasBoard: true,
                isComplete: board.IsComplete,
                deadlocked: SimpleDeadlockDetector.HasStaticCornerDeadlock(board));

            Assert.AreEqual(PuzzleHudStatus.Deadlock, status);
        }

        [Test]
        public void Status_StaysActiveWhileTheCargoCanStillMove()
        {
            PuzzleBoardModel board = BuildBoard(
                crates: new Dictionary<string, GridCoordinate> { ["loose"] = new(2, 2) },
                goals: new[] { new GridCoordinate(3, 3) });

            PuzzleHudStatus status = PuzzleHudStatusResolver.Resolve(
                hasBoard: true,
                isComplete: board.IsComplete,
                deadlocked: SimpleDeadlockDetector.HasStaticCornerDeadlock(board));

            Assert.AreEqual(PuzzleHudStatus.Active, status);
        }

        // -------------------------------------------------------- Extrato --

        [Test]
        public void Statement_KeepsEveryLineWhenUnderTheLevelCeiling()
        {
            PuzzleRunSummary summary = new() { Moves = 30, Medal = 1, ToolsUsed = 1, HintsUsed = 1 };
            IReadOnlyList<ShiftReportLine> lines =
                ShiftReportPresenter.BuildLines(ShiftCredits.BuildStatement(summary));

            CollectionAssert.AreEqual(
                new[] { "TURNO CONCLUÍDO", "MEDALHA BRONZE" },
                lines.Select(line => line.Label).ToArray());
            Assert.AreEqual(
                ShiftCredits.CompletionReward + ShiftCredits.BronzeReward,
                ShiftReportPresenter.VisibleTotal(lines));
        }

        [Test]
        public void Statement_AddsAnExplicitCapLineWhenBonusesOverflow()
        {
            // Turno perfeito soma 425 em bônus, mas a fase só paga 250. Sem a
            // linha de corte, a soma na tela não fecharia com o crédito recebido.
            PuzzleRunSummary summary = new()
            {
                Moves = 20,
                Medal = 3,
                FirstAttempt = true,
                PersonalBest = true
            };

            IReadOnlyList<CreditEntry> statement = ShiftCredits.BuildStatement(summary);
            IReadOnlyList<ShiftReportLine> lines = ShiftReportPresenter.BuildLines(statement, summary);
            ShiftReportLine capLine = lines[lines.Count - 1];

            Assert.AreEqual(ShiftReportPresenter.CapLabel, capLine.Label);
            Assert.IsTrue(capLine.IsDeduction);
            Assert.AreEqual(statement.Count + 1, lines.Count);
        }

        [Test]
        public void Statement_VisibleTotalAlwaysMatchesWhatTheEconomyPays()
        {
            PuzzleRunSummary[] runs =
            {
                new() { Moves = 20, Medal = 3, FirstAttempt = true, PersonalBest = true },
                new() { Moves = 55, Medal = 2, PersonalBest = true },
                new() { Moves = 90, Medal = 1, ToolsUsed = 2, HintsUsed = 3 },
                new() { Moves = 44, Medal = 0 }
            };

            foreach (PuzzleRunSummary run in runs)
            {
                IReadOnlyList<CreditEntry> statement = ShiftCredits.BuildStatement(run);
                IReadOnlyList<ShiftReportLine> lines = ShiftReportPresenter.BuildLines(statement, run);

                Assert.AreEqual(
                    ShiftCredits.Evaluate(run),
                    ShiftReportPresenter.VisibleTotal(lines),
                    "O extrato na tela precisa somar exatamente o crédito pago.");
                Assert.AreEqual(ShiftCredits.Evaluate(run), ShiftReportPresenter.CappedTotal(statement, run));
            }
        }

        [Test]
        public void Statement_SurvivesAMissingReport()
        {
            // Fase aberta sem SaveManager na cena: a tela ainda aparece, só sem extrato.
            Assert.IsEmpty(ShiftReportPresenter.BuildLines(null));
            Assert.AreEqual(0, ShiftReportPresenter.RawTotal(null));
            Assert.AreEqual(0, ShiftReportPresenter.CappedTotal(null));
            Assert.AreEqual(0, ShiftReportPresenter.VisibleTotal(null));
        }

        [Test]
        public void Statement_LinesCarryTheirSign()
        {
            PuzzleRunSummary summary = new()
            {
                Moves = 20,
                Medal = 3,
                FirstAttempt = true,
                PersonalBest = true
            };

            IReadOnlyList<ShiftReportLine> lines =
                ShiftReportPresenter.BuildLines(ShiftCredits.BuildStatement(summary));

            Assert.AreEqual("+100", lines[0].AmountText);
            StringAssert.StartsWith("-", lines[lines.Count - 1].AmountText);
        }

        [Test]
        public void MedalLabels_CoverEveryTier()
        {
            Assert.AreEqual("MEDALHA PLATINA", ShiftReportPresenter.MedalLabel(3));
            Assert.AreEqual("MEDALHA OURO", ShiftReportPresenter.MedalLabel(2));
            Assert.AreEqual("MEDALHA BRONZE", ShiftReportPresenter.MedalLabel(1));
            Assert.AreEqual(ShiftReportPresenter.EmptyMedalLabel, ShiftReportPresenter.MedalLabel(0));
        }

        [Test]
        public void RankingLabel_SaysWhetherTheRunCounts()
        {
            StringAssert.Contains("VALIDADO", ShiftReportPresenter.RankingLabel(false));
            StringAssert.Contains("FORA DO RANKING", ShiftReportPresenter.RankingLabel(true));
        }

        [Test]
        public void VisibleLineCount_NeverExceedsTheSlotsBuiltIntoTheScene()
        {
            PuzzleRunSummary summary = new()
            {
                Moves = 20,
                Medal = 3,
                FirstAttempt = true,
                PersonalBest = true
            };

            IReadOnlyList<ShiftReportLine> lines =
                ShiftReportPresenter.BuildLines(ShiftCredits.BuildStatement(summary));

            // A cena reserva sete rótulos: seis bônus possíveis mais o corte.
            Assert.LessOrEqual(lines.Count, 7);
            Assert.AreEqual(3, ShiftReportPresenter.VisibleLineCount(lines, 3));
            Assert.AreEqual(lines.Count, ShiftReportPresenter.VisibleLineCount(lines, 7));
            Assert.AreEqual(0, ShiftReportPresenter.VisibleLineCount(lines, 0));
        }

        // --------------------------------------------------------- Apoio --

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

            return new PuzzleBoardModel(5, 5, walls, goals, new GridCoordinate(1, 3), crates);
        }
    }
}
