using NUnit.Framework;
using System.Collections.Generic;

namespace Mapsui.Tests.Navigation;

[TestFixture]
public class PinchTrackerTests
{
    [Test]
    public void Test()
    {
        // Arrange
        var pinchTracker = new PinchTracker();
        int[][] pinch1 = [[0, 0], [1, 0]];
        int[][] pinch2 = [[0, 0], [1, 1]];

        // Act
        pinchTracker.Update(ToListOfMPoint(pinch1));
        pinchTracker.Update(ToListOfMPoint(pinch2));
        var pinchManipulation = pinchTracker.GetPinchManipulation();

        // Assert

        Assert.That(45, Is.EqualTo(pinchManipulation.RotationChange));
    }

    List<MPoint> ToListOfMPoint(int[][] array)
    {
        return new List<MPoint> { new MPoint(array[0][0], array[0][1]), new MPoint(array[1][0], array[1][1]) };
    }
}
