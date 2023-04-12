using Mapsui.Limiting;
using NUnit.Framework;

namespace Mapsui.Tests.UI;

[TestFixture]
public class ViewportLimiterTests
{
    [Test]
    public void TestRestrictZoom()
    {
        // arrange
        var viewport = new Viewport(0, 0, 1, 0, 100, 100);
        // viewport.Center is (0, 0) at this point
        var limiter = new ViewportLimiter();
        var panBounds = new MRect(20, 40, 120, 140);  // Minimal X value is 20, Minimal Y value is 40
        
        // act 
        var result = limiter.Limit(viewport, panBounds, null);

        // assert
        Assert.AreEqual(20, result.CenterX);
        Assert.AreEqual(40, result.CenterY);
    }
}
