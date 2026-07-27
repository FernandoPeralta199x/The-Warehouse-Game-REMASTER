#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using TW08.Save;

namespace TW08.Tests.EditMode
{
    public sealed class SaveIntegrityTests
    {
        [Test]
        public void ChecksumDetectsPayloadChanges()
        {
            const string original = "{\\\"version\\\":1}";
            string checksum = SaveIntegrity.ComputeChecksum(original);

            Assert.That(SaveIntegrity.IsValid(original, checksum), Is.True);
            Assert.That(SaveIntegrity.IsValid(original + "x", checksum), Is.False);
        }
    }
}
#endif
