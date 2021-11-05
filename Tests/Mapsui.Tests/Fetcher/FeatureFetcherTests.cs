using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
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
            var extent = new MRect(0, 0, 10, 10);
            var layer = new Layer
            {
                DataSource = new GeometryMemoryProvider<IGeometryFeature>(GenerateRandomPoints(extent, 25))
            };
            layer.Delayer.MillisecondsToWait = 0;

            var notifications = new List<bool>();
            layer.PropertyChanged += (_, args) => {
                if (args.PropertyName == nameof(Layer.Busy))
                {
                    notifications.Add(layer.Busy);
                }
            };
            var fetchInfo = new FetchInfo(extent, 1, null, ChangeType.Discrete);

            // act
            layer.RefreshData(fetchInfo);

            // assert
            Task.Run(() => {
                while (notifications.Count < 2)
                {
                    // just wait until we have two
                }
            }).GetAwaiter().GetResult();
            Assert.IsTrue(notifications[0]);
            Assert.IsFalse(notifications[1]);
        }

        private static IEnumerable<IGeometry> GenerateRandomPoints(MRect envelope, int count)
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
