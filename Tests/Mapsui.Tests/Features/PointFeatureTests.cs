using Mapsui.Layers;
using NUnit.Framework;

namespace Mapsui.Tests.Features;

[TestFixture]
public class PointFeatureTests
{
    [Test]
    public void Clone_ShouldCopyIdAndData_AndExtent()
    {
        // Arrange
        var x = 5.0;
        var y = 6.0;
        var original = new PointFeature(x, y);
        var data = "SomeData";
        original.Data = data;

        // Act
        var clone = (PointFeature)original.Clone();

        // Assert
        Assert.That(clone.Id, Is.EqualTo(original.Id), "Clone should copy the Id field.");
        Assert.That(clone.Data, Is.EqualTo(data), "Clone should copy the Data field.");
        Assert.That(clone.Extent, Is.Not.Null, "Extent should not be null.");
        Assert.That(clone.Extent.MinX, Is.EqualTo(x), "Extent.MinX should match the point's X.");
        Assert.That(clone.Extent.MaxX, Is.EqualTo(x), "Extent.MaxX should match the point's X.");
        Assert.That(clone.Extent.MinY, Is.EqualTo(y), "Extent.MinY should match the point's Y.");
        Assert.That(clone.Extent.MaxY, Is.EqualTo(y), "Extent.MaxY should match the point's Y.");
    }
}
