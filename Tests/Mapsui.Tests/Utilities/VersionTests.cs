using Mapsui.Utilities;
using NUnit.Framework;
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
        Assert.That(version?.ToString().Length > 0, Is.True);
    }
}
