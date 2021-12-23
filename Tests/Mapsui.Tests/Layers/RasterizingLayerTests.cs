using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task TestTimer()
        {
            // arrange
            DefaultRendererFactory.Create = () => new MapRenderer();
            using var memoryLayer = CreatePointLayer();
            using var layer = new RasterizingLayer(memoryLayer);
            var schema = new GlobalSphericalMercator();
            var box = schema.Extent.ToMRect();
            var resolution = schema.Resolutions.First().Value.UnitsPerPixel;
            using var waitHandle = new AutoResetEvent(false);

            Assert.AreEqual(0, await layer.GetFeatures(box, resolution).CountAsync());
            layer.DataChanged += (_, _) => {
                // assert
                waitHandle.Set();
            };

            var fetchInfo = new FetchInfo(box, resolution, null, ChangeType.Discrete);

            // act
            layer.RefreshData(fetchInfo);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(await layer.GetFeatures(box, resolution).CountAsync(), 1);
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
