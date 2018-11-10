using Mapsui.Geometries;
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
            writableLayer.Add(new Feature());
            writableLayer.Add(new Feature { Geometry = new Point() });
            writableLayer.Add(new Feature { Geometry = new LineString() });
            writableLayer.Add(new Feature { Geometry = new Polygon() });

            // act
            var extents = writableLayer.Envelope;

            // assert
            Assert.IsNull(extents);
        }
    }
}
