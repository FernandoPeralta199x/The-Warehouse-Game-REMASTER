using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TW08.Economy;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Verifica o que o turno registra para a economia: empurrões, uso de
    /// ferramentas e a marca de turno assistido.
    /// </summary>
    public sealed class PuzzleShiftTrackingTests
    {
        private PuzzleLevelDefinition level;
        private GameObject host;
        private PuzzleRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            level = CreateCorridorLevel();
            host = new GameObject("Shift Tracking Runtime");
            runtime = host.AddComponent<PuzzleRuntime>();
            runtime.Configure(level, null, Array.Empty<PuzzleEntityView>());
            runtime.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Object.DestroyImmediate(level);
        }

        [Test]
        public void WalkingWithoutTouchingCargo_DoesNotCountAsPush()
        {
            // Jogador em (1,1), carga em (3,1): andar para a esquerda não empurra nada.
            Assert.IsTrue(runtime.TryMove(GridCoordinate.Left));
            Assert.AreEqual(0, runtime.PushCount);
        }

        [Test]
        public void PushCount_FollowsUndoAndRedo()
        {
            runtime.TryMove(GridCoordinate.Right);   // aproxima
            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right)); // empurra
            Assert.AreEqual(1, runtime.PushCount);

            Assert.IsTrue(runtime.Undo());
            Assert.AreEqual(0, runtime.PushCount, "Desfazer o empurrão precisa devolver a contagem.");

            Assert.IsTrue(runtime.Redo());
            Assert.AreEqual(1, runtime.PushCount);
        }

        [Test]
        public void RestartingTheLevel_ClearsTheWholeShiftRecord()
        {
            runtime.TryMove(GridCoordinate.Right);
            runtime.TryMove(GridCoordinate.Right);
            runtime.RegisterAssistance(isHint: false);
            runtime.RegisterAssistance(isHint: true);

            Assert.AreEqual(1, runtime.PushCount);
            Assert.IsTrue(runtime.IsAssisted);

            runtime.Restart();

            Assert.AreEqual(0, runtime.PushCount);
            Assert.AreEqual(0, runtime.ToolsUsed);
            Assert.AreEqual(0, runtime.HintsUsed);
            Assert.IsFalse(runtime.IsAssisted, "Reiniciar a fase começa um turno limpo.");
        }

        [Test]
        public void HintsAndToolsAreCountedSeparately()
        {
            runtime.RegisterAssistance(isHint: true);
            runtime.RegisterAssistance(isHint: true);
            runtime.RegisterAssistance(isHint: false);

            Assert.AreEqual(2, runtime.HintsUsed);
            Assert.AreEqual(1, runtime.ToolsUsed);

            PuzzleRunSummary summary = runtime.BuildSummary();
            Assert.AreEqual(2, summary.HintsUsed);
            Assert.AreEqual(1, summary.ToolsUsed);
            Assert.IsFalse(summary.IsClean);
        }

        [Test]
        public void BuildSummary_ReportsTheBoardCostAndPushes()
        {
            runtime.TryMove(GridCoordinate.Right);
            runtime.TryMove(GridCoordinate.Right);

            PuzzleRunSummary summary = runtime.BuildSummary();

            Assert.AreEqual(runtime.Board.MoveCount, summary.Moves);
            Assert.AreEqual(1, summary.Pushes);
            Assert.IsTrue(summary.IsClean, "Sem ferramenta e sem dica, o turno continua limpo.");
        }

        private static PuzzleLevelDefinition CreateCorridorLevel()
        {
            PuzzleLevelDefinition definition = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
            SetField(definition, "levelId", "test-shift-tracking");
            SetField(definition, "displayName", "Shift Tracking");
            SetField(definition, "width", 6);
            SetField(definition, "height", 3);
            SetField(definition, "cellSize", 1f);
            SetField(definition, "playerStart", new GridCoordinate(1, 1));
            SetField(definition, "walls", new List<GridCoordinate>());
            SetField(definition, "goals", new List<GridCoordinate> { new(4, 1) });
            SetField(definition, "crates", new List<PuzzleCrateDefinition>
            {
                new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(3, 1))
            });
            return definition;
        }

        private static void SetField<T>(PuzzleLevelDefinition target, string fieldName, T value)
        {
            FieldInfo field = typeof(PuzzleLevelDefinition)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
