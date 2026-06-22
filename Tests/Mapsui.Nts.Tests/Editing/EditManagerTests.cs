using NetTopologySuite.Geometries;
using NUnit.Framework;
using Mapsui.Nts.Editing;

namespace Mapsui.Nts.Tests.Editing;

[TestFixture]
public class EditManagerTests
{
    [Test]
    public void EndEditShouldNotThrowWhenPolygonHasFewerThanThreePoints()
    {
        // Arrange
        var editManager = new EditManager { EditMode = EditMode.AddPolygon };
        editManager.AddVertex(new Coordinate(0, 0)); // Only one point placed before finishing.

        // Act & Assert
        Assert.DoesNotThrow(() => editManager.EndEdit());
    }
}
