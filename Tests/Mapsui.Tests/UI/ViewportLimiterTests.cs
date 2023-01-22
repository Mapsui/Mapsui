using Mapsui.UI;
using NUnit.Framework;

namespace Mapsui.Tests.UI;

[TestFixture]
public class ViewportLimiterTests
{
    [Test]
    public void TestRestrictZoom()
    {
        // arrange
        var viewport = new Viewport { CenterX = 0, CenterY = 0, Width = 100, Height = 100, Resolution = 1 };
        // viewport.Center is (0, 0) at this point
        var limiter = new ViewportLimiter
        {
            PanLimits = new MRect(20, 40, 120, 140)  // Minimal X value is 20, Minimal Y value is 40
        };

        // act 
        limiter.LimitExtent(viewport, viewport.Extent);

        // assert
        Assert.AreEqual(viewport.CenterX, 20);
        Assert.AreEqual(viewport.CenterY, 40);
    }
}
