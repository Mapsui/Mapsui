using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
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
            writableLayer.Add(new GeometryFeature(new Point()));
            writableLayer.Add(new GeometryFeature(new LineString()));
            writableLayer.Add(new GeometryFeature(new Polygon()));

            // act
            var extent = writableLayer.Extent;

            // assert
            Assert.IsNull(extent);
        }
    }
}
