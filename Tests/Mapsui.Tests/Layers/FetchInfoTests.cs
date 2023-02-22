using Mapsui.Layers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
internal class FetchInfoTests
{
    [Test]
    public void GrowFetchInfo()
    {
        // Arrange
        var fetchInfo = new FetchInfo(new MRect(-50, -50, 50, 50), 1);

        // Act
        var grownFetchInfo = fetchInfo.Grow(10);

        // Assert
        Assert.AreEqual(120, grownFetchInfo.Extent.Width);
    }
}
