using Mapsui.Extensions;
using Mapsui.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Extensions;

[TestFixture]
public class ViewportExtensionsTests
{
    [Test]
    public void ToExtentWidthIsZeroWhenResolutionIsZeroTest()
    {
        // Arrange
        var viewport = new Viewport(0, 0, 0, 0, 100, 100);

        // Act
        var extent = viewport.ToExtent();

        // Assert
        Assert.AreEqual(0, extent.Width);
        Assert.AreEqual(0, extent.Height);
        Assert.AreEqual(0, extent.GetArea());
    }

    const double halfEpsilon = Constants.Epsilon * 0.5;
    [Test]
    [TestCase(0, false)]    
    
    [TestCase(1, true)]
    [TestCase(-1, true)]

    // A very small difference should be ignore
    [TestCase(halfEpsilon, false)]
    [TestCase(-halfEpsilon, false)]

    // A slightly bigger difference should not be ignore
    [TestCase(1 + halfEpsilon, true)]
    [TestCase(-1 - halfEpsilon, true)]

    // A very small difference should also be ignore for multiples of 360
    [TestCase(720 + halfEpsilon, false)]
    [TestCase(720 - halfEpsilon, false)]
    [TestCase(-720 + halfEpsilon, false)]
    [TestCase(-720 - halfEpsilon, false)]

    // Bigger values in steps of 180
    [TestCase(180, true)]
    [TestCase(360, false)]
    [TestCase(540, true)]
    [TestCase(720, false)]
    [TestCase(900, true)]
    [TestCase(1080, false)]

    // Bigger negative values in steps of 180
    [TestCase(-180, true)]
    [TestCase(-360, false)]
    [TestCase(-540, true)]
    [TestCase(-720, false)]
    [TestCase(-900, true)]    
    [TestCase(-1080, false)]
    public void IsRotatedTest(double rotation, bool IsRotated)
    {
        // Arrange
        var viewport = new Viewport(0, 0, 0, rotation, 100, 100);

        // Act
        var result = viewport.IsRotated();

        // Assert
        Assert.AreEqual(IsRotated, result);
    }
}
