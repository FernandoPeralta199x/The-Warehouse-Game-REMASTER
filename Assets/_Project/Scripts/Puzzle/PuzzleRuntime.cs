using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TW08.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class PuzzleRuntime : MonoBehaviour
    {
        [SerializeField] private PuzzleLevelDefinition level;
        [SerializeField] private PuzzleEntityView playerView;
        [SerializeField] private List<PuzzleEntityView> crateViews = new();
        [SerializeField] private bool initializeOnStart = true;

        private readonly PuzzleHistory history = new();
        private Dictionary<string, PuzzleEntityView> crateViewById = new();

        public PuzzleBoardModel Board { get; private set; }
        public PuzzleLevelDefinition Level => level;
        public int UndoCount => history.UndoCount;
        public int RedoCount => history.RedoCount;

        public event Action<PuzzleMove> MoveApplied;
        public event Action<PuzzleMove> MoveUndone;
        public event Action<PuzzleMove> MoveRedone;
        public event Action LevelRestarted;
        public event Action LevelCompleted;
        public event Action StaticDeadlockDetected;

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        public void Configure(PuzzleLevelDefinition definition, PuzzleEntityView player, IEnumerable<PuzzleEntityView> crates)
        {
            level = definition;
            playerView = player;
            crateViews = crates?.ToList() ?? new List<PuzzleEntityView>();
        }

        public void Initialize()
        {
            IReadOnlyList<string> validationErrors = PuzzleLevelValidator.Validate(level);
            if (validationErrors.Count > 0)
            {
                Debug.LogError("Puzzle level is invalid:\n- " + string.Join("\n- ", validationErrors), level);
                return;
            }

            Board = new PuzzleBoardModel(level);
            history.Clear();
            crateViewById = crateViews
                .Where(view => view != null && !string.IsNullOrWhiteSpace(view.EntityId))
                .GroupBy(view => view.EntityId)
                .ToDictionary(group => group.Key, group => group.First());
            SyncViews();
        }

        public bool TryMove(GridCoordinate direction)
        {
            if (Board == null || Board.IsComplete || !Board.TryMove(direction, out PuzzleMove move))
            {
                return false;
            }

            history.Record(move);
            SyncViews();
            MoveApplied?.Invoke(move);

            if (Board.IsComplete)
            {
                LevelCompleted?.Invoke();
            }
            else if (SimpleDeadlockDetector.HasStaticCornerDeadlock(Board))
            {
                StaticDeadlockDetected?.Invoke();
            }

            return true;
        }

        public bool Undo()
        {
            if (Board == null || !history.TryPopUndo(out PuzzleMove move))
            {
                return false;
            }

            if (!Board.TryUndo(move))
            {
                history.RestoreUndo(move);
                return false;
            }

            history.PushRedo(move);
            SyncViews();
            MoveUndone?.Invoke(move);
            return true;
        }

        public bool Redo()
        {
            if (Board == null || !history.TryPopRedo(out PuzzleMove move))
            {
                return false;
            }

            GridCoordinate direction = move.PlayerTo - move.PlayerFrom;
            if (!Board.TryMove(direction, out PuzzleMove repeated))
            {
                history.PushRedo(move);
                return false;
            }

            history.RestoreUndo(repeated);
            SyncViews();
            MoveRedone?.Invoke(repeated);
            return true;
        }

        public void Restart()
        {
            Initialize();
            LevelRestarted?.Invoke();
        }

        private void SyncViews()
        {
            if (Board == null)
            {
                return;
            }

            playerView?.Snap(Board.PlayerPosition, level.CellSize);
            foreach (KeyValuePair<GridCoordinate, string> crate in Board.Crates)
            {
                if (crateViewById.TryGetValue(crate.Value, out PuzzleEntityView view))
                {
                    view.Snap(crate.Key, level.CellSize);
                }
            }
        }
    }
}
