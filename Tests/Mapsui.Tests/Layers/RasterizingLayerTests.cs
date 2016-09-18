using System;
using System.Linq;
using BruTile;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;
using BruTile.Predefined;

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class RasterizingLayerTests
    {
        [Test]
        public void TestTimer()
        {
            // arrange
            var layer = new RasterizingLayer(CreatePointLayer());
            var schema = new GlobalSphericalMercator();
            var box = schema.Extent.ToBoundingBox();
            var resolution = schema.Resolutions.First().Value.UnitsPerPixel;

            Assert.AreEqual(0, layer.GetFeaturesInView(box, resolution).Count());
            layer.DataChanged += (sender, args) =>
            {
                // assert
                Assert.AreSame(layer.GetFeaturesInView(box, resolution).Count(), 1);
            };

            // act
            layer.ViewChanged(true, box, resolution);
        }

        private static MemoryLayer CreatePointLayer()
        {
            var provider = new MemoryProvider();
            var random = new Random();
            for (var i = 0; i < 10000; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(random.Next(100000, 5000000), random.Next(100000, 5000000))
                };
                provider.Features.Add(feature);
            }
            return new MemoryLayer { DataSource = provider };
        }
    }
}
