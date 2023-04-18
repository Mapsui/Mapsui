using Mapsui.Extensions;
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
}
