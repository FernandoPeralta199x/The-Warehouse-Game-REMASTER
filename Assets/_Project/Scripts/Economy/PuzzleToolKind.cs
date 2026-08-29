namespace TW08.Economy
{
    /// <summary>
    /// Ferramentas da Oficina N-8 disponíveis dentro de uma fase de puzzle.
    /// Apenas o conjunto de MVP definido na bíblia de design: ferramentas que
    /// ajudam o jogador a pensar ou corrigir um erro, nunca a pular o desafio.
    /// </summary>
    public enum PuzzleToolKind
    {
        None = 0,

        /// <summary>Desfaz os últimos 3 movimentos de uma vez.</summary>
        RewindMove = 1,

        /// <summary>Destaca por alguns segundos a carga em situação mais crítica.</summary>
        LogisticsScanner = 2,

        /// <summary>Dicas em camadas, da mais vaga à mais direta.</summary>
        ShiftAssistant = 3,

        /// <summary>Marca no piso as células de alvo ainda descobertas.</summary>
        RouteMarker = 4
    }

    /// <summary>Raridade da ferramenta — controla apresentação e preço na Oficina N-8.</summary>
    public enum PuzzleToolRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3
    }
}
