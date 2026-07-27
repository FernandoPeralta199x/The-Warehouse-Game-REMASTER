using System;
using System.Collections.Generic;

namespace TW08.Puzzle
{
    public static class PuzzleLevelValidator
    {
        public static IReadOnlyList<string> Validate(PuzzleLevelDefinition level)
        {
            List<string> errors = new();
            if (level == null)
            {
                errors.Add("Level definition is null.");
                return errors;
            }

            if (level.Width <= 0 || level.Height <= 0)
            {
                errors.Add("Board dimensions must be positive.");
            }

            HashSet<GridCoordinate> occupied = new();
            foreach (GridCoordinate wall in level.Walls)
            {
                if (!IsInside(level, wall))
                {
                    errors.Add($"Wall {wall} is outside the board.");
                }

                if (!occupied.Add(wall))
                {
                    errors.Add($"Duplicate wall at {wall}.");
                }
            }

            if (!IsInside(level, level.PlayerStart))
            {
                errors.Add("Player start is outside the board.");
            }
            else if (occupied.Contains(level.PlayerStart))
            {
                errors.Add("Player starts on a wall.");
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<GridCoordinate> cratePositions = new();
            foreach (PuzzleCrateDefinition crate in level.Crates)
            {
                if (crate == null)
                {
                    errors.Add("Level contains a null crate.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(crate.Id) || !ids.Add(crate.Id))
                {
                    errors.Add($"Crate id '{crate.Id}' is empty or duplicated.");
                }

                if (!IsInside(level, crate.Position))
                {
                    errors.Add($"Crate '{crate.Id}' is outside the board.");
                }
                else if (occupied.Contains(crate.Position) || !cratePositions.Add(crate.Position))
                {
                    errors.Add($"Crate '{crate.Id}' overlaps another blocked entity at {crate.Position}.");
                }
            }

            HashSet<GridCoordinate> goals = new(level.Goals);
            if (goals.Count != level.Goals.Count)
            {
                errors.Add("Level contains duplicate goals.");
            }

            foreach (GridCoordinate goal in goals)
            {
                if (!IsInside(level, goal))
                {
                    errors.Add($"Goal {goal} is outside the board.");
                }
            }

            if (level.Crates.Count != level.Goals.Count)
            {
                errors.Add("Standard puzzle levels require the same number of crates and goals.");
            }

            return errors;
        }

        private static bool IsInside(PuzzleLevelDefinition level, GridCoordinate cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < level.Width && cell.Y < level.Height;
        }
    }
}
