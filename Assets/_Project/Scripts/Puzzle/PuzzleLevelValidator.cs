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
            IReadOnlyList<PuzzleGoalRequirementDefinition> goalRequirements = level.GoalRequirements ?? Array.Empty<PuzzleGoalRequirementDefinition>();
            IReadOnlyList<PuzzleCrateDefinition> crates = level.Crates ?? Array.Empty<PuzzleCrateDefinition>();
            IReadOnlyList<GridCoordinate> costlyCells = level.CostlyCells ?? Array.Empty<GridCoordinate>();
            IReadOnlyList<PuzzleSwitchGroupDefinition> switchGroups = level.SwitchGroups ?? Array.Empty<PuzzleSwitchGroupDefinition>();

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

            ValidateGoalRequirements(level, goalRequirements, goals, errors);
            ValidateCostlyCells(level, costlyCells, wallCells, errors);
            ValidateSwitchGroups(level, switchGroups, wallCells, errors);
            return errors;
        }

        private static void ValidateGoalRequirements(
            PuzzleLevelDefinition level,
            IReadOnlyList<PuzzleGoalRequirementDefinition> requirements,
            HashSet<GridCoordinate> goals,
            List<string> errors)
        {
            HashSet<GridCoordinate> unique = new();
            foreach (PuzzleGoalRequirementDefinition requirement in requirements)
            {
                if (requirement == null)
                {
                    errors.Add("Level contains a null goal requirement.");
                    continue;
                }

                if (!IsInside(level, requirement.Position))
                {
                    errors.Add($"Goal requirement {requirement.Position} is outside the board.");
                }
                else if (!goals.Contains(requirement.Position))
                {
                    errors.Add($"Goal requirement {requirement.Position} does not reference a goal.");
                }

                if (requirement.RequiredKind == PuzzleEntityKind.Player)
                {
                    errors.Add($"Goal requirement {requirement.Position} cannot require the Player kind.");
                }

                if (!unique.Add(requirement.Position))
                {
                    errors.Add($"Duplicate goal requirement at {requirement.Position}.");
                }
            }
        }

        private static void ValidateCostlyCells(
            PuzzleLevelDefinition level,
            IReadOnlyList<GridCoordinate> costlyCells,
            HashSet<GridCoordinate> wallCells,
            List<string> errors)
        {
            HashSet<GridCoordinate> unique = new();
            foreach (GridCoordinate cell in costlyCells)
            {
                if (!IsInside(level, cell))
                {
                    errors.Add($"Costly cell {cell} is outside the board.");
                }
                else if (wallCells.Contains(cell))
                {
                    errors.Add($"Costly cell {cell} overlaps a wall.");
                }

                if (!unique.Add(cell))
                {
                    errors.Add($"Duplicate costly cell at {cell}.");
                }
            }
        }

        private static void ValidateSwitchGroups(
            PuzzleLevelDefinition level,
            IReadOnlyList<PuzzleSwitchGroupDefinition> groups,
            HashSet<GridCoordinate> wallCells,
            List<string> errors)
        {
            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<GridCoordinate> allDoors = new();

            foreach (PuzzleSwitchGroupDefinition group in groups)
            {
                if (group == null)
                {
                    errors.Add("Level contains a null switch group.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(group.Id) || !ids.Add(group.Id))
                {
                    errors.Add($"Switch group id '{group.Id}' is empty or duplicated.");
                }

                if (group.Sensors == null || group.Sensors.Count == 0)
                {
                    errors.Add($"Switch group '{group.Id}' requires at least one sensor.");
                }

                if (group.Doors == null || group.Doors.Count == 0)
                {
                    errors.Add($"Switch group '{group.Id}' requires at least one door.");
                }

                ValidateMechanicCells(level, group.Id, "sensor", group.Sensors, wallCells, null, errors);
                ValidateMechanicCells(level, group.Id, "door", group.Doors, wallCells, allDoors, errors);
            }
        }

        private static void ValidateMechanicCells(
            PuzzleLevelDefinition level,
            string groupId,
            string label,
            IReadOnlyList<GridCoordinate> cells,
            HashSet<GridCoordinate> wallCells,
            HashSet<GridCoordinate> globalUnique,
            List<string> errors)
        {
            if (cells == null)
            {
                return;
            }

            HashSet<GridCoordinate> local = new();
            foreach (GridCoordinate cell in cells)
            {
                if (!IsInside(level, cell))
                {
                    errors.Add($"Switch group '{groupId}' {label} {cell} is outside the board.");
                }
                else if (wallCells.Contains(cell))
                {
                    errors.Add($"Switch group '{groupId}' {label} {cell} overlaps a wall.");
                }

                if (!local.Add(cell))
                {
                    errors.Add($"Switch group '{groupId}' contains duplicate {label} {cell}.");
                }

                if (globalUnique != null && !globalUnique.Add(cell))
                {
                    errors.Add($"Door {cell} is assigned to more than one switch group.");
                }
            }
        }

        private static bool IsInside(PuzzleLevelDefinition level, GridCoordinate cell)
        {
            return cell.X >= 0 && cell.Y >= 0 && cell.X < level.Width && cell.Y < level.Height;
        }
    }
}
