using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Fetcher
{
    [TestFixture]
    public class FeatureFetcherTests
    {
        [Test]
        public void TestFeatureFetcherDelay()
        {
            // arrange
            var extent = new BoundingBox(0, 0, 10, 10);
            var layer = new Layer
            {
                DataSource = new MemoryProvider(GenerateRandomPoints(extent, 25)),
                FetchingPostponedInMilliseconds = 0
            };

            // act
            layer.ViewChanged(true, extent, 1);   

            // assert
            Task.Run(() => 
            {
                while (!layer.Busy)
                {
                    Assert.IsFalse(layer.Busy); 
                }
            }).GetAwaiter().GetResult();
            Assert.IsTrue(layer.Busy);
        }

        private static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox envelope, int count)
        {
            var random = new Random();
            var result = new List<IGeometry>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new Point(
                    random.NextDouble() * envelope.Width + envelope.Left,
                    random.NextDouble() * envelope.Height + envelope.Bottom));
            }

            return result;
        }
    }
}
