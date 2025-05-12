using Mapsui.Limiting;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.UI;

[TestFixture]
public class ViewportLimiterKeepWithinExtentTests
{
    [Test]
    public void TestRestrictZoom()
    {
        // arrange
        var viewport1 = new Viewport(12.61510114915, 41.8676902329, 0.014662864685125001, 0, 800, 600);
        var viewport2 = new Viewport(12.61510114915, 41.8676902329, 0.017492343139666661, 0, 800, 600);

        // viewport.Center is (0, 0) at this point
        var limiter = new ViewportLimiterKeepWithinExtent();
        var panBounds = GetLimitsOfItaly();

        // act 
        var result1 = limiter.Limit(viewport1, panBounds, null);
        var result2 = limiter.Limit(viewport2, panBounds, null);

        // assert
        ClassicAssert.AreEqual(result2, result1);
    }

    private static MRect GetLimitsOfItaly()
    {
        var (minX, minY) = (6.7499552751, 36.619987291);
        var (maxX, maxY) = (18.4802470232, 47.1153931748);
        return new MRect(minX, minY, maxX, maxY);
    }
}
