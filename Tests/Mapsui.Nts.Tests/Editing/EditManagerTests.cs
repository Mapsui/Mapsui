using System.Linq;
using Mapsui.Layers;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Mapsui.Nts.Editing;

namespace Mapsui.Nts.Tests.Editing;

[TestFixture]
public class EditManagerTests
{
    [Test]
    public void EndEditShouldCancelInvalidPolygonAndNotifyLayer()
    {
        var layer = new WritableLayer();
        var editManager = new EditManager { EditMode = EditMode.AddPolygon, Layer = layer };
        var dataChangedCount = 0;
        layer.DataChanged += (_, _) => dataChangedCount++;

        editManager.AddVertex(new Coordinate(0, 0)); // Only one point placed before finishing.

        Assert.DoesNotThrow(() => editManager.EndEdit());
        Assert.That(editManager.EditMode, Is.EqualTo(EditMode.AddPolygon));
        Assert.That(layer.GetFeatures().Count(), Is.EqualTo(0));
        Assert.That(dataChangedCount, Is.EqualTo(2)); // one add, one cancellation
    }

    [Test]
    public void EndEditShouldCancelInvalidLineAndNotifyLayer()
    {
        var layer = new WritableLayer();
        var editManager = new EditManager { EditMode = EditMode.AddLine, Layer = layer };
        var dataChangedCount = 0;
        layer.DataChanged += (_, _) => dataChangedCount++;

        editManager.AddVertex(new Coordinate(0, 0)); // Only one point placed before finishing.

        Assert.DoesNotThrow(() => editManager.EndEdit());
        Assert.That(editManager.EditMode, Is.EqualTo(EditMode.AddLine));
        Assert.That(layer.GetFeatures().Count(), Is.EqualTo(0));
        Assert.That(dataChangedCount, Is.EqualTo(2)); // one add, one cancellation
    }
}
