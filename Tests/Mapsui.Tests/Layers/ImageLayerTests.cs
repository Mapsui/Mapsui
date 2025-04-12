using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class ImageLayerTests
{
    private const string ExceptionMessage = "This exception should return on OnDataChange";

    private class FakeProvider : IProvider
    {
        public string? CRS { get; set; }
        public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
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
        using var imageLayer = new ImageLayer("imageLayer") { DataSource = provider };
        using var map = new Map();
        map.Layers.Add(imageLayer);
        using var waitHandle = new AutoResetEvent(false);
        Exception? exception = null;

        imageLayer.DataChanged += (s, e) =>
        {
            exception = e.Error;
            waitHandle.Go();
        };

        var fetchInfo = new FetchInfo(new MSection(new MRect(-1, -1, 0, 0), 1), null, ChangeType.Discrete);
        var viewport = new Viewport(-0.5, -0.5, 1, 0, 1, 1);

        // act
        map.RefreshData(viewport);

        // assert
        waitHandle.WaitOne();
        ClassicAssert.AreEqual(ExceptionMessage, exception?.Message);
    }
}
