using NUnit.Framework;

namespace Mapsui.Tests.Navigation;

[TestFixture]
public class PinchTrackerTests
{
    [Test]
    public void Test()
    {
        // Arrange
        var pinchTracker = new PinchTracker();
        pinchTracker.Update([new(0, 0), new(1, 0)]);
        pinchTracker.Update([new(0, 0), new(1, 1)]);

        // Act
        var pinchManipulation = pinchTracker.GetPinchManipulation();

        // Assert
        Assert.That(45, Is.EqualTo(pinchManipulation.RotationChange));
    }
}
