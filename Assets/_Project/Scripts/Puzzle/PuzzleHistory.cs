using System.Collections.Generic;

namespace TW08.Puzzle
{
    public sealed class PuzzleHistory
    {
        private readonly Stack<PuzzleMove> undo = new();
        private readonly Stack<PuzzleMove> redo = new();

        public int UndoCount => undo.Count;
        public int RedoCount => redo.Count;

        public void Record(PuzzleMove move)
        {
            undo.Push(move);
            redo.Clear();
        }

        public bool TryPopUndo(out PuzzleMove move)
        {
            if (undo.Count == 0)
            {
                move = default;
                return false;
            }

            move = undo.Pop();
            return true;
        }

        public void PushRedo(PuzzleMove move)
        {
            redo.Push(move);
        }

        public bool TryPopRedo(out PuzzleMove move)
        {
            if (redo.Count == 0)
            {
                move = default;
                return false;
            }

            move = redo.Pop();
            return true;
        }

        public void RestoreUndo(PuzzleMove move)
        {
            undo.Push(move);
        }

        public void Clear()
        {
            undo.Clear();
            redo.Clear();
        }
    }
}
