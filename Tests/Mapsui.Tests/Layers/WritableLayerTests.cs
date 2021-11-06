using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class WritableLayerTests
    {
        [Test]
        public void DoNotCrashOnNullOrEmptyGeometries()
        {
            // arrange
            var writableLayer = new WritableLayer();
            writableLayer.Add(new GeometryFeature());
            writableLayer.Add(new GeometryFeature { Geometry = new Point() });
            writableLayer.Add(new GeometryFeature { Geometry = new LineString() });
            writableLayer.Add(new GeometryFeature { Geometry = new Polygon() });

            // act
            var extent = writableLayer.Extent;

            // assert
            Assert.IsNull(extent);
        }
    }
}
