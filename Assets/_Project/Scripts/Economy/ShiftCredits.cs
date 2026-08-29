using System.Collections.Generic;

namespace TW08.Economy
{
    /// <summary>Uma linha do extrato de Créditos de Turno.</summary>
    public readonly struct CreditEntry
    {
        public CreditEntry(string label, int amount)
        {
            Label = label;
            Amount = amount;
        }

        public string Label { get; }
        public int Amount { get; }
    }

    /// <summary>
    /// Converte o resultado de um turno em Créditos de Turno.
    ///
    /// Os valores vêm da tabela de ganhos da bíblia de design. O teto por fase
    /// existe porque a mesma bíblia trata como requisito que uma fase comum
    /// renda entre 100 e 250 créditos: sem ele o jogador compraria a loja
    /// inteira no primeiro setor e a Oficina N-8 perderia a função.
    ///
    /// A escala de medalhas do jogo é bronze/ouro/platina (1/2/3) e recebe a
    /// curva 25/50/100 da tabela original.
    /// </summary>
    public static class ShiftCredits
    {
        public const int CompletionReward = 100;
        public const int BronzeReward = 25;
        public const int GoldReward = 50;
        public const int PlatinumReward = 100;
        public const int NoToolsReward = 50;
        public const int NoHintsReward = 50;
        public const int PersonalBestReward = 75;
        public const int FirstAttemptReward = 50;
        public const int SectorClearReward = 300;

        /// <summary>Teto de créditos que uma única fase pode render.</summary>
        public const int MaxPerLevel = 250;

        /// <summary>Total de créditos ganhos no turno, já com o teto aplicado.</summary>
        public static int Evaluate(PuzzleRunSummary summary)
        {
            int total = 0;
            foreach (CreditEntry entry in BuildStatement(summary))
            {
                total += entry.Amount;
            }

            return total > MaxPerLevel ? MaxPerLevel : total;
        }

        /// <summary>
        /// Extrato detalhado, na ordem em que é exibido na tela de resultado.
        /// Não aplica o teto: quem exibe mostra as linhas e o total já limitado.
        /// </summary>
        public static IReadOnlyList<CreditEntry> BuildStatement(PuzzleRunSummary summary)
        {
            List<CreditEntry> entries = new() { new CreditEntry("TURNO CONCLUÍDO", CompletionReward) };

            switch (summary.Medal)
            {
                case 3:
                    entries.Add(new CreditEntry("MEDALHA PLATINA", PlatinumReward));
                    break;
                case 2:
                    entries.Add(new CreditEntry("MEDALHA OURO", GoldReward));
                    break;
                case 1:
                    entries.Add(new CreditEntry("MEDALHA BRONZE", BronzeReward));
                    break;
            }

            if (summary.ToolsUsed <= 0)
            {
                entries.Add(new CreditEntry("SEM FERRAMENTAS", NoToolsReward));
            }

            if (summary.HintsUsed <= 0)
            {
                entries.Add(new CreditEntry("SEM DICAS", NoHintsReward));
            }

            if (summary.PersonalBest)
            {
                entries.Add(new CreditEntry("NOVO RECORDE", PersonalBestReward));
            }

            if (summary.FirstAttempt)
            {
                entries.Add(new CreditEntry("PRIMEIRA TENTATIVA", FirstAttemptReward));
            }

            return entries;
        }
    }
}
