using Mapsui.Geometries;
using Mapsui.UI;
using NUnit.Framework;

namespace Mapsui.Tests.UI
{
    [TestFixture]
    public class ViewportLimiterTests
    {
        [Test]
        public void TestRestrictZoom()
        {
            // arrange
            var viewport = new Viewport { Center = new Point(0, 0), Width = 100, Height = 100, Resolution = 1};
            // viewport.Center is (0, 0) at this point
            var restrictTo = new BoundingBox(20, 40, 120, 140); // Minimal X value is 20, Minimal Y value is 40

            // act 
            ViewportLimiter.LimitExtent(viewport, PanMode.KeepCenterWithinExtents, restrictTo, viewport.Extent);

            // assert
            Assert.AreEqual(viewport.Center.X, 20);
            Assert.AreEqual(viewport.Center.Y, 40);
        }
    }
}
