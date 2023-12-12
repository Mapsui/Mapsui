using Mapsui.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Utilities;

[TestFixture]
public class VersionTests
{
    [Test]
    public void GetCurrentVersion()
    {
        // act
        var version = Version.GetCurrentVersion();

        // assert
        ClassicAssert.True(version?.ToString().Length > 0);
    }
}
