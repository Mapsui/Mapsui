using System;
using System.Collections.Generic;
using System.Threading;
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

        class FakeProvider : IProvider
        {
            public string CRS { get; set; }
            public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
            {
                throw new Exception(ExceptionMessage);
            }

            public BoundingBox GetExtents()
            {
                return new BoundingBox(-1, -1, 0, 0);
            }
        }

        [Test]
        public void TestExceptionOnProvider()
        {
            // arrange
            var provider = new FakeProvider();
            var imageLayer = new ImageLayer("imageLayer") { DataSource = provider};
            var map = new Map();
            map.Layers.Add(imageLayer);
            var waitHandle = new AutoResetEvent(false);
            Exception exception = null;

            imageLayer.DataChanged += (sender, args) =>
            {
                exception = args.Error;
                waitHandle.Go();
            };

            // act
            map.RefreshData(new BoundingBox(-1, -1, 0, 0), 1, ChangeType.Discrete);

            // assert
            waitHandle.WaitOne();
            Assert.AreEqual(ExceptionMessage, exception.Message);
        }
    }
}
