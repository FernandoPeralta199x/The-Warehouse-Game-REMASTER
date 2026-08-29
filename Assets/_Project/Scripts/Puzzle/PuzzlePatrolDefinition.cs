using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>
    /// Robô de limpeza que percorre uma rota fixa, um passo por comando do
    /// jogador.
    ///
    /// A rota é uma lista de células em ordem e volta ao início ao terminar, o
    /// que torna a posição do robô função apenas do número de comandos dados.
    /// Isso é o que mantém a fase determinística e o solver capaz de provar a
    /// solução: bastam a rota e um contador, sem simulação paralela.
    /// </summary>
    [Serializable]
    public sealed class PuzzlePatrolDefinition
    {
        [SerializeField] private string patrolId = "robot-01";
        [SerializeField] private List<GridCoordinate> route = new();

        public PuzzlePatrolDefinition()
        {
        }

        public PuzzlePatrolDefinition(string patrolId, IEnumerable<GridCoordinate> route)
        {
            this.patrolId = patrolId;
            this.route = new List<GridCoordinate>(route ?? Array.Empty<GridCoordinate>());
        }

        public string PatrolId => patrolId;
        public IReadOnlyList<GridCoordinate> Route => route;

        /// <summary>Onde o robô está após <paramref name="step"/> comandos.</summary>
        public GridCoordinate PositionAt(int step)
        {
            if (route == null || route.Count == 0)
            {
                return default;
            }

            // Passo negativo aparece ao desfazer: o resto em C# herda o sinal do
            // dividendo, então some o módulo antes de indexar.
            int index = step % route.Count;
            if (index < 0)
            {
                index += route.Count;
            }

            return route[index];
        }
    }
}
