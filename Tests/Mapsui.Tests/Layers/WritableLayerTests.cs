using Mapsui.Geometries;
using Mapsui.GeometryLayers;
using Mapsui.Layers;
using NUnit.Framework;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class WritableLayerTests
    {
        [Test]
        public void DoNotCrashOnNullOrEmptyGeometries()
        {
            // arrange
            using var writableLayer = new WritableLayer();
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
