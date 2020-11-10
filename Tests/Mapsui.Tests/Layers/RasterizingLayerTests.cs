using System;
using System.Linq;
using System.Threading;
using BruTile;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;
using BruTile.Predefined;
using Mapsui.Rendering;
using Mapsui.Rendering.Xaml;

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
            var waitHandle = new AutoResetEvent(false);
            DefaultRendererFactory.Create = () => new MapRenderer(); // Using xaml renderer here to test rasterizer. Suboptimal. 
            
            Assert.AreEqual(0, layer.GetFeaturesInView(box, resolution).Count());
            layer.DataChanged += (sender, args) =>
            {
                // assert
                waitHandle.Set();
            };

            // act
            layer.RefreshData(box, resolution, ChangeType.Discrete);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(layer.GetFeaturesInView(box, resolution).Count(), 1);
        }

        private static MemoryLayer CreatePointLayer()
        {
            var random = new Random();
            var features = new Features();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Feature
                {
                    Geometry = new Geometries.Point(random.Next(100000, 5000000), random.Next(100000, 5000000))
                };
                features.Add(feature);
            }
            var provider = new MemoryProvider(features);

            return new MemoryLayer { DataSource = provider };
        }
    }
}
