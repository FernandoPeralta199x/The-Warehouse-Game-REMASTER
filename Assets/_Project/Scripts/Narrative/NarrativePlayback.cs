using System;
using System.Collections.Generic;

namespace TW08.Narrative
{
    /// <summary>
    /// Cursor de leitura de uma sequência.
    ///
    /// Classe pura, sem Unity: é ela quem responde "qual fala está na tela" e
    /// "acabou?". Linhas vazias ou nulas são puladas na entrada, então um asset
    /// meio preenchido nunca trava a cutscene em uma caixa de texto em branco.
    /// </summary>
    public sealed class NarrativePlayback
    {
        private readonly IReadOnlyList<NarrativeLine> lines;
        private int index;

        public NarrativePlayback(IReadOnlyList<NarrativeLine> source)
        {
            lines = source ?? Array.Empty<NarrativeLine>();
            SkipBlank();
        }

        public int Index => index;
        public int LineCount => lines.Count;
        public bool IsFinished => index >= lines.Count;
        public NarrativeLine Current => IsFinished ? null : lines[index];

        /// <summary>Avança uma fala. Retorna false quando a sequência terminou.</summary>
        public bool Advance()
        {
            if (IsFinished)
            {
                return false;
            }

            index++;
            SkipBlank();
            return !IsFinished;
        }

        public void Rewind()
        {
            index = 0;
            SkipBlank();
        }

        private void SkipBlank()
        {
            while (index < lines.Count && (lines[index] == null || !lines[index].HasText))
            {
                index++;
            }
        }
    }
}
