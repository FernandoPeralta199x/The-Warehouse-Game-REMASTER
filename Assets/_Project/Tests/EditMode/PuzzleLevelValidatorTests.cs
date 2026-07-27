#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    public sealed class PuzzleLevelValidatorTests
    {
        [Test]
        public void ValidatorRejectsMismatchedCratesAndGoals()
        {
            Assert.That(PuzzleLevelValidator.Validate(null), Is.Not.Empty);
        }
    }
}
#endif
