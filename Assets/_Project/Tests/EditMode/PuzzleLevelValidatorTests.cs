#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    public sealed class PuzzleLevelValidatorTests
    {
        [Test]
        public void ValidatorRejectsNullDefinition()
        {
            Assert.That(PuzzleLevelValidator.Validate(null), Is.Not.Empty);
        }

        [Test]
        public void ValidatorAcceptsSimpleValidPuzzle()
        {
            PuzzleLevelDefinition level = CreateLevel(
                player: new GridCoordinate(1, 1),
                walls: new List<GridCoordinate>(),
                goals: new List<GridCoordinate> { new(3, 1) },
                crates: new List<PuzzleCrateDefinition>
                {
                    new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(2, 1))
                });

            try
            {
                Assert.That(PuzzleLevelValidator.Validate(level), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void ValidatorRejectsGoalOnWall()
        {
            GridCoordinate blocked = new(3, 1);
            PuzzleLevelDefinition level = CreateLevel(
                player: new GridCoordinate(1, 1),
                walls: new List<GridCoordinate> { blocked },
                goals: new List<GridCoordinate> { blocked },
                crates: new List<PuzzleCrateDefinition>
                {
                    new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(2, 1))
                });

            try
            {
                Assert.That(
                    PuzzleLevelValidator.Validate(level).Any(error => error.Contains("Goal") && error.Contains("wall")),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void ValidatorRejectsCrateOnPlayerStart()
        {
            GridCoordinate start = new(1, 1);
            PuzzleLevelDefinition level = CreateLevel(
                player: start,
                walls: new List<GridCoordinate>(),
                goals: new List<GridCoordinate> { new(3, 1) },
                crates: new List<PuzzleCrateDefinition>
                {
                    new("crate-a", PuzzleEntityKind.Crate, start)
                });

            try
            {
                Assert.That(
                    PuzzleLevelValidator.Validate(level).Any(error => error.Contains("player start")),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void ValidatorRejectsEmptyPuzzle()
        {
            PuzzleLevelDefinition level = CreateLevel(
                player: new GridCoordinate(1, 1),
                walls: new List<GridCoordinate>(),
                goals: new List<GridCoordinate>(),
                crates: new List<PuzzleCrateDefinition>());

            try
            {
                IReadOnlyList<string> errors = PuzzleLevelValidator.Validate(level);
                Assert.That(errors.Any(error => error.Contains("at least one crate")), Is.True);
                Assert.That(errors.Any(error => error.Contains("at least one goal")), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        [Test]
        public void ValidatorRejectsMismatchedCratesAndGoals()
        {
            PuzzleLevelDefinition level = CreateLevel(
                player: new GridCoordinate(1, 1),
                walls: new List<GridCoordinate>(),
                goals: new List<GridCoordinate> { new(3, 1), new(3, 2) },
                crates: new List<PuzzleCrateDefinition>
                {
                    new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(2, 1))
                });

            try
            {
                Assert.That(
                    PuzzleLevelValidator.Validate(level).Any(error => error.Contains("same number of crates and goals")),
                    Is.True);
            }
            finally
            {
                Object.DestroyImmediate(level);
            }
        }

        private static PuzzleLevelDefinition CreateLevel(
            GridCoordinate player,
            List<GridCoordinate> walls,
            List<GridCoordinate> goals,
            List<PuzzleCrateDefinition> crates)
        {
            PuzzleLevelDefinition level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
            SetField(level, "levelId", "validator-test");
            SetField(level, "displayName", "Validator Test");
            SetField(level, "width", 5);
            SetField(level, "height", 5);
            SetField(level, "cellSize", 1f);
            SetField(level, "playerStart", player);
            SetField(level, "walls", walls);
            SetField(level, "goals", goals);
            SetField(level, "crates", crates);
            return level;
        }

        private static void SetField<T>(PuzzleLevelDefinition target, string fieldName, T value)
        {
            FieldInfo field = typeof(PuzzleLevelDefinition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
#endif
