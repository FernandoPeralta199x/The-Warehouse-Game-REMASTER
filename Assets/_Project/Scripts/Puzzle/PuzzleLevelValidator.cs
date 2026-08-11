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

            if (level.Width < 3 || level.Height < 3)
            {
                errors.Add("Board dimensions must be at least 3x3.");
            }

            IReadOnlyList<GridCoordinate> walls = level.Walls ?? Array.Empty<GridCoordinate>();
            IReadOnlyList<GridCoordinate> goalsSource = level.Goals ?? Array.Empty<GridCoordinate>();
            IReadOnlyList<PuzzleCrateDefinition> crates = level.Crates ?? Array.Empty<PuzzleCrateDefinition>();

            HashSet<GridCoordinate> wallCells = new();
            foreach (GridCoordinate wall in walls)
            {
                if (!IsInside(level, wall))
                {
                    errors.Add($"Wall {wall} is outside the board.");
                }

                if (!wallCells.Add(wall))
                {
                    errors.Add($"Duplicate wall at {wall}.");
                }
            }

            if (!IsInside(level, level.PlayerStart))
            {
                errors.Add("Player start is outside the board.");
            }
            else if (wallCells.Contains(level.PlayerStart))
            {
                errors.Add("Player starts on a wall.");
            }

            if (crates.Count == 0)
            {
                errors.Add("Standard puzzle levels require at least one crate.");
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<GridCoordinate> cratePositions = new();
            foreach (PuzzleCrateDefinition crate in crates)
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

                if (crate.Kind == PuzzleEntityKind.Player)
                {
                    errors.Add($"Crate '{crate.Id}' cannot use the Player entity kind.");
                }

                if (!IsInside(level, crate.Position))
                {
                    errors.Add($"Crate '{crate.Id}' is outside the board.");
                    continue;
                }

                if (wallCells.Contains(crate.Position))
                {
                    errors.Add($"Crate '{crate.Id}' overlaps a wall at {crate.Position}.");
                }

                if (crate.Position == level.PlayerStart)
                {
                    errors.Add($"Crate '{crate.Id}' overlaps the player start at {crate.Position}.");
                }

                if (!cratePositions.Add(crate.Position))
                {
                    errors.Add($"Crate '{crate.Id}' overlaps another crate at {crate.Position}.");
                }
            }

            if (goalsSource.Count == 0)
            {
                errors.Add("Standard puzzle levels require at least one goal.");
            }

            HashSet<GridCoordinate> goals = new(goalsSource);
            if (goals.Count != goalsSource.Count)
            {
                errors.Add("Level contains duplicate goals.");
            }

            foreach (GridCoordinate goal in goals)
            {
                if (!IsInside(level, goal))
                {
                    errors.Add($"Goal {goal} is outside the board.");
                }
                else if (wallCells.Contains(goal))
                {
                    errors.Add($"Goal {goal} overlaps a wall.");
                }
            }

            if (crates.Count != goalsSource.Count)
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
