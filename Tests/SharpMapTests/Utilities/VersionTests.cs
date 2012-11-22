
using NUnit.Framework;
using SharpMap.Utilities;

namespace SharpMapTests.Utilities
{
    [TestFixture]
    public class VersionTests
    {
        [Test]
        public void GetCurrentVersion()
        {
            // act
            var version = Version.GetCurrentVersion();

            // assert
            Assert.True(version.ToString().Length > 0);
        }
    }
}
