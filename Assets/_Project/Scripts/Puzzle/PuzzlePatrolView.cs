using System.Collections.Generic;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>
    /// Mostra os robôs de limpeza no tabuleiro.
    ///
    /// A posição nunca é simulada aqui: ela é lida de
    /// <see cref="PuzzleBoardModel.CommandCount"/>, que é a mesma fonte que o
    /// modelo usa para recusar movimentos. Uma vista com relógio próprio ficaria
    /// fora de sincronia com a regra depois do primeiro desfazer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzlePatrolView : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<Transform> robots = new();
        [SerializeField, Min(1f)] private float followSpeed = 11f;

        private float cellSize = 1f;

        public void Configure(PuzzleRuntime puzzleRuntime, IEnumerable<Transform> robotTransforms, float cell)
        {
            runtime = puzzleRuntime;
            robots = robotTransforms != null ? new List<Transform>(robotTransforms) : new List<Transform>();
            cellSize = Mathf.Max(0.1f, cell);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (runtime?.Level != null)
            {
                cellSize = Mathf.Max(0.1f, runtime.Level.CellSize);
            }

            SnapToBoard();

            if (runtime != null)
            {
                runtime.Initialized += SnapToBoard;
                runtime.LevelRestarted += SnapToBoard;
            }
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.Initialized -= SnapToBoard;
                runtime.LevelRestarted -= SnapToBoard;
            }
        }

        private void Update()
        {
            PuzzleBoardModel board = runtime?.Board;
            if (board == null)
            {
                return;
            }

            IReadOnlyList<PuzzlePatrolDefinition> patrols = board.Patrols;
            float step = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            for (int i = 0; i < robots.Count && i < patrols.Count; i++)
            {
                if (robots[i] == null)
                {
                    continue;
                }

                Vector3 target = patrols[i].PositionAt(board.CommandCount).ToWorld(cellSize);
                robots[i].position = Vector3.Lerp(robots[i].position, target, step);
            }
        }

        /// <summary>
        /// Coloca cada robô direto na célula correta, sem interpolar. Usado ao
        /// abrir e ao reiniciar a fase, quando não existe movimento a suavizar.
        /// </summary>
        private void SnapToBoard()
        {
            PuzzleBoardModel board = runtime?.Board;
            if (board == null)
            {
                return;
            }

            IReadOnlyList<PuzzlePatrolDefinition> patrols = board.Patrols;
            for (int i = 0; i < robots.Count && i < patrols.Count; i++)
            {
                if (robots[i] != null)
                {
                    robots[i].position = patrols[i].PositionAt(board.CommandCount).ToWorld(cellSize);
                }
            }
        }
    }
}
