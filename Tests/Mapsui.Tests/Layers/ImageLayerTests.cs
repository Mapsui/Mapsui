using System;
using System.Collections.Generic;
using System.Threading;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class ImageLayerTests
    {
        private const string ExceptionMessage = "This exception should return on OnDataChange";

        class FakeProvider : IProvider<IGeometryFeature>
        {
            public string? CRS { get; set; }
            public IEnumerable<IGeometryFeature> GetFeatures(FetchInfo fetchInfo)
            {
                throw new Exception(ExceptionMessage);
            }

            public MRect GetExtent()
            {
                return new MRect(-1, -1, 0, 0);
            }
        }

        [Test]
        public void TestExceptionOnProvider()
        {
            // arrange
            var provider = new FakeProvider();
            var imageLayer = new ImageLayer("imageLayer") { DataSource = provider };
            var map = new Map();
            map.Layers.Add(imageLayer);
            var waitHandle = new AutoResetEvent(false);
            Exception? exception = null;

            imageLayer.DataChanged += (_, args) => {
                exception = args.Error;
                waitHandle.Go();
            };

            var fetchInfo = new FetchInfo
            {
                Extent = new MRect(-1, -1, 0, 0),
                Resolution = 1,
                ChangeType = ChangeType.Discrete
            };

            // act
            map.RefreshData(fetchInfo);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(ExceptionMessage, exception.Message);
        }
    }
}
