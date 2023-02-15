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
        var viewport = new Viewport (0, 0, 1, 0, 100, 100);
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
