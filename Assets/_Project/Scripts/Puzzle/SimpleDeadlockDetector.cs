namespace TW08.Puzzle
{
    public static class SimpleDeadlockDetector
    {
        public static bool HasStaticCornerDeadlock(PuzzleBoardModel board)
        {
            if (board == null)
            {
                return false;
            }

            foreach (GridCoordinate crate in board.Crates.Keys)
            {
                if (board.IsGoal(crate))
                {
                    continue;
                }

                bool up = IsBlocked(board, crate + GridCoordinate.Up);
                bool down = IsBlocked(board, crate + GridCoordinate.Down);
                bool left = IsBlocked(board, crate + GridCoordinate.Left);
                bool right = IsBlocked(board, crate + GridCoordinate.Right);

                if ((up || down) && (left || right))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBlocked(PuzzleBoardModel board, GridCoordinate cell)
        {
            return !board.IsInside(cell) || board.IsWall(cell);
        }
    }
}
