using NUnit.Framework;
using TW08.Presentation;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    public sealed class FacingDirectionTests
    {
        [Test]
        public void CardinalDeltasMapToExpectedFacing()
        {
            Assert.That(FacingDirectionUtility.FromDelta(Vector2Int.up), Is.EqualTo(FacingDirection.Up));
            Assert.That(FacingDirectionUtility.FromDelta(Vector2Int.down), Is.EqualTo(FacingDirection.Down));
            Assert.That(FacingDirectionUtility.FromDelta(Vector2Int.left), Is.EqualTo(FacingDirection.Left));
            Assert.That(FacingDirectionUtility.FromDelta(Vector2Int.right), Is.EqualTo(FacingDirection.Right));
        }

        [Test]
        public void ZeroDeltaPreservesFallbackFacing()
        {
            FacingDirection result = FacingDirectionUtility.FromDelta(Vector2Int.zero, FacingDirection.Left);
            Assert.That(result, Is.EqualTo(FacingDirection.Left));
        }

        [Test]
        public void DominantAxisWinsForNonCardinalInput()
        {
            Assert.That(FacingDirectionUtility.FromDelta(new Vector2Int(3, 1)), Is.EqualTo(FacingDirection.Right));
            Assert.That(FacingDirectionUtility.FromDelta(new Vector2Int(-4, 1)), Is.EqualTo(FacingDirection.Left));
            Assert.That(FacingDirectionUtility.FromDelta(new Vector2Int(1, 5)), Is.EqualTo(FacingDirection.Up));
        }
    }
}
