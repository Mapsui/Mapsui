using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using NUnit.Framework;

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
            var box = schema.Extent.ToMRect();
            var resolution = schema.Resolutions.First().Value.UnitsPerPixel;
            var waitHandle = new AutoResetEvent(false);
            DefaultRendererFactory.Create = () => new MapRenderer(); // Using xaml renderer here to test rasterizer. Suboptimal. 

            Assert.AreEqual(0, layer.GetFeatures(box, resolution).Count());
            layer.DataChanged += (_, _) => {
                // assert
                waitHandle.Set();
            };

            var fetchInfo = new FetchInfo
            {
                Extent = box,
                Resolution = resolution,
                ChangeType = ChangeType.Discrete
            };

            // act
            layer.RefreshData(fetchInfo);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(layer.GetFeatures(box, resolution).Count(), 1);
        }

        private static MemoryLayer CreatePointLayer()
        {
            var random = new Random();
            var features = new List<IGeometryFeature>();
            for (var i = 0; i < 100; i++)
            {
                var feature = new GeometryFeature
                {
                    Geometry = new Geometries.Point(random.Next(100000, 5000000), random.Next(100000, 5000000))
                };
                features.Add(feature);
            }
            var provider = new GeometryMemoryProvider<IGeometryFeature>(features);

            return new MemoryLayer { DataSource = provider };
        }
    }
}
