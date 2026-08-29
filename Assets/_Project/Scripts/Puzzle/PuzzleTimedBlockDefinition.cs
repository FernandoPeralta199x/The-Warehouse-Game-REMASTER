using System;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>
    /// Bloqueio que se retira sozinho depois de um número de comandos.
    ///
    /// Modelado como prazo, e não como duração: a célula está fechada enquanto o
    /// turno não chegar a <see cref="OpensAfterCommands"/> e a partir daí fica
    /// aberta para sempre. É essa forma que mantém o estado finito — o solver só
    /// precisa saber se o prazo já passou, não contar para sempre.
    /// </summary>
    [Serializable]
    public sealed class PuzzleTimedBlockDefinition
    {
        [SerializeField] private GridCoordinate position;
        [SerializeField, Min(1)] private int opensAfterCommands = 6;

        public PuzzleTimedBlockDefinition()
        {
        }

        public PuzzleTimedBlockDefinition(GridCoordinate position, int opensAfterCommands)
        {
            this.position = position;
            this.opensAfterCommands = Mathf.Max(1, opensAfterCommands);
        }

        public GridCoordinate Position => position;
        public int OpensAfterCommands => Mathf.Max(1, opensAfterCommands);

        public bool IsClosedAt(int commandCount) => commandCount < OpensAfterCommands;
    }
}
