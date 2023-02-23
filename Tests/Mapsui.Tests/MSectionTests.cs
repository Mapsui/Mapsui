using Mapsui.Extensions;
using NUnit.Framework;

namespace Mapsui.Tests;

[TestFixture]
internal class MSectionTests
{
    [TestCase(1d, 1d, 1d, 100d)]
    [TestCase(2d, 1d, 1d, 200d)]
    [TestCase(4d, 1d, 1d, 400d)]
    [TestCase(1d, 2d, 0.5d, 100d)]
    [TestCase(2d, 2d, 0.5d, 200d)]
    [TestCase(4d, 2d, 0.5d, 400d)]
    [TestCase(1d, 4d, 0.25d, 100d)]
    [TestCase(2d, 4d, 0.25d, 200d)]
    [TestCase(4d, 4d, 0.25d, 400d)]
    public void OverscanTest(double extentMultiplier, double resolutionMultiplier, double expectedResolution, double expectedWidth)
    {
        // Arrange
        var section = new MSection(new MRect(-50, -50, 50, 50), 1);

        // Act
        var grownSection = section.Multiply(extentMultiplier, resolutionMultiplier);

        // Assert
        Assert.AreEqual(expectedResolution, grownSection.Resolution);
        Assert.AreEqual(expectedWidth, grownSection.Extent.Width);
    }
}
