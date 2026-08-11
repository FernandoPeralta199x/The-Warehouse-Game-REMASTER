using System.Collections;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;
using UnityEngine.TestTools;

namespace TW08.Tests.PlayMode
{
    public sealed class PuzzleEntityViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator AnimatedMoveReachesLogicalTarget()
        {
            GameObject gameObject = new("Entity View Test");
            PuzzleEntityView view = gameObject.AddComponent<PuzzleEntityView>();
            view.Snap(new GridCoordinate(0, 0), 1f);

            view.MoveTo(new GridCoordinate(1, 0), 1f, true);
            yield return new WaitForSeconds(0.15f);

            Assert.That(gameObject.transform.position.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(gameObject.transform.position.y, Is.EqualTo(0f).Within(0.001f));
            Object.Destroy(gameObject);
        }

        [Test]
        public void NonAnimatedMoveSnapsImmediately()
        {
            GameObject gameObject = new("Entity View Test");
            PuzzleEntityView view = gameObject.AddComponent<PuzzleEntityView>();

            view.MoveTo(new GridCoordinate(2, 3), 1f, false);

            Assert.That(gameObject.transform.position.x, Is.EqualTo(2f).Within(0.001f));
            Assert.That(gameObject.transform.position.y, Is.EqualTo(3f).Within(0.001f));
            Object.DestroyImmediate(gameObject);
        }
    }
}
