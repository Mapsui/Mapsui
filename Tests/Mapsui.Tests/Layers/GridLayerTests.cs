using Mapsui.Layers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class GridLayerTests
{
    [Test]
    public void GetFeatures_ReturnsEmpty()
    {
        using var layer = new GridLayer();

        var features = layer.GetFeatures(new MRect(-1, -1, 1, 1), 1);

        Assert.That(features, Is.Empty);
    }

    [Test]
    public void Extent_IsNull()
    {
        using var layer = new GridLayer();

        Assert.That(layer.Extent, Is.Null);
    }

    [Test]
    public void CustomLayerRendererName_IsSet()
    {
        using var layer = new GridLayer();

        Assert.That(layer.CustomLayerRendererName, Is.EqualTo(GridLayer.LayerRendererName));
    }

    [Test]
    public void DefaultProperties_AreReasonable()
    {
        using var layer = new GridLayer();

        Assert.That(layer.TargetLineCount, Is.GreaterThan(0));
        Assert.That(layer.LineWidth, Is.GreaterThan(0));
        Assert.That(layer.LabelSize, Is.GreaterThan(0));
        Assert.That(layer.ShowCoordinateLabels, Is.False);
    }

    [Test]
    public void NameConstructor_SetsName()
    {
        using var layer = new GridLayer("My Grid");

        Assert.That(layer.Name, Is.EqualTo("My Grid"));
    }
}
