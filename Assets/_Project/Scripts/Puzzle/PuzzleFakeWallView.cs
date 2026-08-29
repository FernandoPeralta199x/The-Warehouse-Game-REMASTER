using System.Collections.Generic;
using TW08.Motion;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>
    /// Paredes falsas: células livres que se apresentam como parede até o
    /// operador chegar perto o bastante para notar a fresta.
    ///
    /// A mentira é só visual. No tabuleiro a célula sempre foi passagem, e é por
    /// isso que as fases com parede falsa não precisaram ser reprovadas: o
    /// solver nunca viu uma parede ali.
    ///
    /// Revelar por proximidade, e não ao atravessar, é deliberado — o jogador
    /// precisa poder descobrir o segredo olhando, sem ter que tentar andar
    /// contra cada parede do setor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleFakeWallView : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<GridCoordinate> cells = new();
        [SerializeField] private List<SpriteRenderer> disguises = new();
        [SerializeField, Min(1)] private int revealDistance = 1;
        [SerializeField, Range(0f, 1f)] private float revealedAlpha = 0.18f;

        private readonly HashSet<GridCoordinate> revealed = new();
        private readonly List<MotionHandle> handles = new();

        public void Configure(
            PuzzleRuntime puzzleRuntime,
            IEnumerable<GridCoordinate> fakeCells,
            IEnumerable<SpriteRenderer> renderers)
        {
            runtime = puzzleRuntime;
            cells = fakeCells != null ? new List<GridCoordinate>(fakeCells) : new List<GridCoordinate>();
            disguises = renderers != null ? new List<SpriteRenderer>(renderers) : new List<SpriteRenderer>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private void OnEnable()
        {
            if (runtime != null)
            {
                runtime.MoveApplied += OnMoved;
                runtime.MoveUndone += OnMoved;
                runtime.MoveRedone += OnMoved;
                runtime.LevelRestarted += OnRestarted;
            }

            CheckProximity();
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.MoveApplied -= OnMoved;
                runtime.MoveUndone -= OnMoved;
                runtime.MoveRedone -= OnMoved;
                runtime.LevelRestarted -= OnRestarted;
            }

            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
        }

        private void OnMoved(PuzzleMove _) => CheckProximity();

        private void OnRestarted()
        {
            // Reiniciar a fase esconde de novo: a descoberta faz parte do
            // desafio e seria perdida se ficasse marcada para sempre.
            revealed.Clear();
            for (int i = 0; i < disguises.Count; i++)
            {
                if (disguises[i] != null)
                {
                    Color color = disguises[i].color;
                    color.a = 1f;
                    disguises[i].color = color;
                }
            }
        }

        private void CheckProximity()
        {
            if (runtime?.Board == null)
            {
                return;
            }

            GridCoordinate player = runtime.Board.PlayerPosition;

            for (int i = 0; i < cells.Count && i < disguises.Count; i++)
            {
                GridCoordinate cell = cells[i];
                if (revealed.Contains(cell))
                {
                    continue;
                }

                int distance = Mathf.Abs(cell.X - player.X) + Mathf.Abs(cell.Y - player.Y);
                if (distance > revealDistance)
                {
                    continue;
                }

                revealed.Add(cell);
                SpriteRenderer disguise = disguises[i];
                if (disguise != null)
                {
                    handles.Add(UIMotion.FadeTo(disguise, revealedAlpha, 0.45f, Ease.OutQuad));
                }
            }
        }
    }
}
