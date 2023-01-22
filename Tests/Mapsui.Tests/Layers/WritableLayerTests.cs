using Mapsui.Layers;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using NUnit.Framework;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Tests.Layers;

[TestFixture]
public class WritableLayerTests
{
    [Test]
    public void DoNotCrashOnNullOrEmptyGeometries()
    {
        // arrange
        using var writableLayer = new WritableLayer();
        writableLayer.Add(new GeometryFeature());
        writableLayer.Add(new GeometryFeature((Point?)null));
        writableLayer.Add(new GeometryFeature((LineString?)null));
        writableLayer.Add(new GeometryFeature((Polygon?)null));
        // act
        var extent = writableLayer.Extent;

        // assert
        Assert.IsNull(extent);
    }
}
