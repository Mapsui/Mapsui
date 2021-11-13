using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BruTile.Predefined;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
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
            DefaultRendererFactory.Create = () => new MapRenderer();
            var layer = new RasterizingLayer(CreatePointLayer());
            var schema = new GlobalSphericalMercator();
            var box = schema.Extent.ToMRect();
            var resolution = schema.Resolutions.First().Value.UnitsPerPixel;
            var waitHandle = new AutoResetEvent(false);

            Assert.AreEqual(0, layer.GetFeatures(box, resolution).Count());
            layer.DataChanged += (_, _) => {
                // assert
                waitHandle.Set();
            };

            var fetchInfo = new FetchInfo(box, resolution, null, ChangeType.Discrete);

            // act
            layer.RefreshData(fetchInfo);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(layer.GetFeatures(box, resolution).Count(), 1);
        }

        private static MemoryLayer CreatePointLayer()
        {
            var random = new Random();
            var features = new List<IFeature>();
            for (var i = 0; i < 100; i++)
            {
                features.Add(new GeometryFeature(
                    new Point(random.Next(100000, 5000000), random.Next(100000, 5000000))));
            }
            var provider = new MemoryProvider<IFeature>(features);

            return new MemoryLayer { DataSource = provider };
        }
    }
}
