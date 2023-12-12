using Mapsui.Layers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Layers;

[TestFixture]
internal class FetchInfoTests
{
    [TestCase(10, 0.5, 120)]
    [TestCase(10, 1, 140)]
    [TestCase(10, 2, 180)]
    public void GrowFetchInfo(double amount, double resolution, double expectedWidth)
    {
        // Arrange
        var fetchInfo = new FetchInfo(new MSection(new MRect(-50, -50, 50, 50), resolution));

        // Act
        var grownFetchInfo = fetchInfo.Grow(amount);

        // Assert
        ClassicAssert.AreEqual(expectedWidth, grownFetchInfo.Extent.Width);
    }
}
