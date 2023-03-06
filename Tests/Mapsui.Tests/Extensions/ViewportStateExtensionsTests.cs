using Mapsui.Extensions;
using NUnit.Framework;

namespace Mapsui.Tests.Extensions;

[TestFixture]
public class ViewportStateExtensionsTests
{
    [Test]
    public void ToExtentWidthIsZeroWhenResolutionIsZeroTest()
    {
        // Arrange
        var viewportState = new ViewportState(0, 0, 0, 0, 100, 100);

        // Act
        var extent = viewportState.ToExtent();

        // Assert
        Assert.AreEqual(0, extent.Width);
        Assert.AreEqual(0, extent.Height);
        Assert.AreEqual(0, extent.GetArea());

    }
}
