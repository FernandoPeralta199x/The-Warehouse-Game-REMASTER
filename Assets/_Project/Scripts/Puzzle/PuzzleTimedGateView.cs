using System.Collections.Generic;
using TW08.Motion;
using UnityEngine;

namespace TW08.Puzzle
{
    /// <summary>
    /// Mostra os portões temporizados, que se retiram quando o turno chega ao
    /// prazo de cada um.
    ///
    /// O estado vem sempre de <see cref="PuzzleBoardModel.CommandCount"/>, a
    /// mesma fonte que o modelo usa para bloquear a célula. Uma vista com
    /// contador próprio sairia de sincronia no primeiro desfazer e o jogador
    /// veria um portão aberto que o motor ainda considera fechado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleTimedGateView : MonoBehaviour
    {
        [SerializeField] private PuzzleRuntime runtime;
        [SerializeField] private List<SpriteRenderer> gates = new();

        private readonly List<bool> shownClosed = new();
        private readonly List<MotionHandle> handles = new();

        public void Configure(PuzzleRuntime puzzleRuntime, IEnumerable<SpriteRenderer> gateRenderers)
        {
            runtime = puzzleRuntime;
            gates = gateRenderers != null ? new List<SpriteRenderer>(gateRenderers) : new List<SpriteRenderer>();

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
                runtime.MoveApplied += OnBoardChanged;
                runtime.MoveUndone += OnBoardChanged;
                runtime.MoveRedone += OnBoardChanged;
                runtime.Initialized += Sync;
                runtime.LevelRestarted += Sync;
            }

            shownClosed.Clear();
            Sync();
        }

        private void OnDisable()
        {
            if (runtime != null)
            {
                runtime.MoveApplied -= OnBoardChanged;
                runtime.MoveUndone -= OnBoardChanged;
                runtime.MoveRedone -= OnBoardChanged;
                runtime.Initialized -= Sync;
                runtime.LevelRestarted -= Sync;
            }

            foreach (MotionHandle handle in handles)
            {
                handle?.Complete();
            }

            handles.Clear();
        }

        private void OnBoardChanged(PuzzleMove _) => Sync();

        private void Sync()
        {
            PuzzleBoardModel board = runtime?.Board;
            if (board == null)
            {
                return;
            }

            IReadOnlyList<PuzzleTimedBlockDefinition> blocks = board.TimedBlocks;

            while (shownClosed.Count < gates.Count)
            {
                shownClosed.Add(true);
            }

            for (int i = 0; i < gates.Count && i < blocks.Count; i++)
            {
                if (gates[i] == null || blocks[i] == null)
                {
                    continue;
                }

                bool closed = blocks[i].IsClosedAt(board.CommandCount);
                if (closed == shownClosed[i])
                {
                    continue;
                }

                shownClosed[i] = closed;
                handles.Add(UIMotion.FadeTo(gates[i], closed ? 0.85f : 0f, 0.35f, Ease.OutQuad));
            }
        }
    }
}
