using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests
{
    public sealed class PuzzleRuntimeTests
    {
        [Test]
        public void RedoWinningMoveRaisesCompletionAgain()
        {
            PuzzleLevelDefinition level = CreateSinglePushLevel();
            GameObject runtimeObject = new("Puzzle Runtime Test");
            PuzzleRuntime runtime = runtimeObject.AddComponent<PuzzleRuntime>();
            runtime.Configure(level, null, Array.Empty<PuzzleEntityView>());
            runtime.Initialize();

            int completionCount = 0;
            runtime.LevelCompleted += () => completionCount++;

            Assert.That(runtime.TryMove(GridCoordinate.Right), Is.True);
            Assert.That(runtime.Board.IsComplete, Is.True);
            Assert.That(completionCount, Is.EqualTo(1));

            Assert.That(runtime.Undo(), Is.True);
            Assert.That(runtime.Board.IsComplete, Is.False);

            Assert.That(runtime.Redo(), Is.True);
            Assert.That(runtime.Board.IsComplete, Is.True);
            Assert.That(completionCount, Is.EqualTo(2));

            UnityEngine.Object.DestroyImmediate(runtimeObject);
            UnityEngine.Object.DestroyImmediate(level);
        }

        private static PuzzleLevelDefinition CreateSinglePushLevel()
        {
            PuzzleLevelDefinition level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
            SetField(level, "levelId", "test-redo-win");
            SetField(level, "displayName", "Redo Win Test");
            SetField(level, "width", 5);
            SetField(level, "height", 3);
            SetField(level, "cellSize", 1f);
            SetField(level, "playerStart", new GridCoordinate(1, 1));
            SetField(level, "walls", new List<GridCoordinate>());
            SetField(level, "goals", new List<GridCoordinate> { new(3, 1) });
            SetField(level, "crates", new List<PuzzleCrateDefinition>
            {
                new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(2, 1))
            });
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
