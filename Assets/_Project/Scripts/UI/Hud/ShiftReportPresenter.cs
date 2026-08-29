using System.Collections.Generic;
using TW08.Economy;

namespace TW08.UI.Hud
{
    /// <summary>Uma linha já formatada do extrato de fim de turno.</summary>
    public readonly struct ShiftReportLine
    {
        public ShiftReportLine(string label, int amount)
        {
            Label = label;
            Amount = amount;
        }

        public string Label { get; }
        public int Amount { get; }

        /// <summary>Valor com sinal, como aparece à direita da linha.</summary>
        public string AmountText => HudFormat.Signed(Amount);

        /// <summary>Linhas negativas são descontos e recebem cor de alerta.</summary>
        public bool IsDeduction => Amount < 0;
    }

    /// <summary>
    /// Monta a tela de conclusão de turno a partir do extrato da economia.
    ///
    /// <see cref="ShiftCredits.BuildStatement"/> devolve os bônus brutos, sem o
    /// teto por fase; quem paga é <see cref="ShiftCredits.Evaluate"/>. Se o HUD
    /// mostrasse só as linhas brutas, a soma na tela não bateria com o crédito
    /// recebido — por isso o corte entra como uma linha explícita.
    /// </summary>
    public static class ShiftReportPresenter
    {
        public const string CapLabel = "TETO DA FASE";
        public const string EmptyMedalLabel = "SEM MEDALHA";

        public static IReadOnlyList<ShiftReportLine> BuildLines(IReadOnlyList<CreditEntry> statement)
        {
            return BuildLines(statement, ShiftCredits.MaxPerLevel);
        }

        /// <summary>
        /// Versão que conhece o turno, e portanto o teto certo.
        ///
        /// O teto depende da medalha: com um teto único, bronze e platina numa
        /// zerada limpa pagavam o mesmo e a soma na tela não fechava.
        /// </summary>
        public static IReadOnlyList<ShiftReportLine> BuildLines(
            IReadOnlyList<CreditEntry> statement, PuzzleRunSummary summary)
        {
            return BuildLines(statement, ShiftCredits.CapFor(summary.Medal));
        }

        public static IReadOnlyList<ShiftReportLine> BuildLines(
            IReadOnlyList<CreditEntry> statement, int cap)
        {
            List<ShiftReportLine> lines = new();
            if (statement == null)
            {
                return lines;
            }

            int raw = 0;
            foreach (CreditEntry entry in statement)
            {
                lines.Add(new ShiftReportLine(entry.Label, entry.Amount));
                raw += entry.Amount;
            }

            if (raw > cap)
            {
                lines.Add(new ShiftReportLine(CapLabel, cap - raw));
            }

            return lines;
        }

        /// <summary>Soma sem teto, do jeito que a economia calcula os bônus.</summary>
        public static int RawTotal(IReadOnlyList<CreditEntry> statement)
        {
            if (statement == null)
            {
                return 0;
            }

            int total = 0;
            foreach (CreditEntry entry in statement)
            {
                total += entry.Amount;
            }

            return total;
        }

        /// <summary>Soma que o jogador realmente recebe — igual ao que a economia credita.</summary>
        public static int CappedTotal(IReadOnlyList<CreditEntry> statement)
        {
            return CappedTotal(statement, ShiftCredits.MaxPerLevel);
        }

        public static int CappedTotal(IReadOnlyList<CreditEntry> statement, PuzzleRunSummary summary)
        {
            return CappedTotal(statement, ShiftCredits.CapFor(summary.Medal));
        }

        public static int CappedTotal(IReadOnlyList<CreditEntry> statement, int cap)
        {
            int raw = RawTotal(statement);
            return raw > cap ? cap : raw;
        }

        /// <summary>Somatório das linhas exibidas — precisa fechar com o crédito pago.</summary>
        public static int VisibleTotal(IReadOnlyList<ShiftReportLine> lines)
        {
            if (lines == null)
            {
                return 0;
            }

            int total = 0;
            foreach (ShiftReportLine line in lines)
            {
                total += line.Amount;
            }

            return total;
        }

        public static string MedalLabel(int medal)
        {
            return medal switch
            {
                3 => "MEDALHA PLATINA",
                2 => "MEDALHA OURO",
                1 => "MEDALHA BRONZE",
                _ => EmptyMedalLabel
            };
        }

        public static string RankingLabel(bool assisted)
        {
            return assisted
                ? "TURNO ASSISTIDO // FORA DO RANKING"
                : "TURNO LIMPO // RANKING VALIDADO";
        }

        /// <summary>Quantas linhas do extrato cabem antes de precisar de rolagem.</summary>
        public static int VisibleLineCount(IReadOnlyList<ShiftReportLine> lines, int slotCount)
        {
            if (lines == null || slotCount <= 0)
            {
                return 0;
            }

            return lines.Count < slotCount ? lines.Count : slotCount;
        }
    }
}
