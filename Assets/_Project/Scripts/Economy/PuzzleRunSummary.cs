using System;

namespace TW08.Economy
{
    /// <summary>
    /// Fotografia do turno recém-concluído. É a entrada do cálculo de
    /// Créditos de Turno e do relatório mostrado ao jogador.
    /// </summary>
    [Serializable]
    public struct PuzzleRunSummary
    {
        /// <summary>Custo total em movimentos (piso custoso conta 2).</summary>
        public int Moves;

        /// <summary>Quantas vezes uma carga foi empurrada.</summary>
        public int Pushes;

        /// <summary>Ferramentas da Oficina N-8 acionadas no turno.</summary>
        public int ToolsUsed;

        /// <summary>Dicas do Assistente de Turno reveladas.</summary>
        public int HintsUsed;

        /// <summary>Medalha conquistada: 1 bronze, 2 ouro, 3 platina.</summary>
        public int Medal;

        /// <summary>True quando é a primeira vez que o jogador entra nesta fase.</summary>
        public bool FirstAttempt;

        /// <summary>True quando o resultado bateu o melhor registro anterior.</summary>
        public bool PersonalBest;

        /// <summary>
        /// Turno limpo: nenhuma ferramenta e nenhuma dica.
        /// Só turnos limpos entram no ranking competitivo.
        /// </summary>
        public bool IsClean => ToolsUsed <= 0 && HintsUsed <= 0;
    }
}
