using NUnit.Framework;
namespace Mapsui.Tests.Utilities;

[TestFixture]
internal class RotationCalculationTests
{
    [TestCase(270, 270)]
    [TestCase(630, 270)]
    [TestCase(-270, 90)]
    [TestCase(-630, 90)]
    [TestCase(360, 0)]
    public static void TestNormalizeRotation(double inputRotation, double expectedRotation)
    {
        // Act
        var rotation = RotationSnapper.NormalizeRotation(inputRotation);

        // Assert
        Assert.That(rotation, Is.EqualTo(expectedRotation));
    }

    [TestCase(90, 180, 90)]
    [TestCase(180, 90, 90)]
    [TestCase(350, 5, 15)]
    [TestCase(5, 350, 15)]
    [TestCase(-90, 90, 180)]
    [TestCase(90, -90, 180)]
    [TestCase(-10, 5, 15)]
    [TestCase(10, -5, 15)]
    [TestCase(710, 5, 15)]
    [TestCase(5, 710, 15)]
    [TestCase(-730, 5, 15)]
    [TestCase(5, -730, 15)]

    public static void TestRotationShortestDistance(double inputRotation1, double inputRotation2, double expectedDistance)
    {
        // Act
        var distance = RotationSnapper.RotationShortestDistance(inputRotation1, inputRotation2);

        // Assert
        Assert.That(distance, Is.EqualTo(expectedDistance));
    }

    [TestCase(0, 15, 5, 0, "Still snapped")]
    [TestCase(0, -15, -5, 0, "Still snapped")]
    [TestCase(0, 25, 5, 25, "Unsnap")]
    [TestCase(0, -25, -5, -25, "Unsnap")]
    [TestCase(25, 30, 5, 5, "Still unsnapped")]
    [TestCase(-25, -30, -5, -5, "Still unsnapped")]
    [TestCase(15, 5, -10, -15, "Resnap")]
    [TestCase(-15, -5, 10, 15, "Resnap")]

    public static void TestCalculateRotationDeltaUsingSnapping(double currentRotation, double virtualRotation, double rotationDelta, double expectedRotationDelta, string message)
    {
        // Arrange
        double unSnapRotation = 20;
        double reSnapRotation = 10;

        // Act
        var adjustedRotationDelta = RotationSnapper.AdjustRotationDeltaForSnapping(rotationDelta, currentRotation, virtualRotation, unSnapRotation, reSnapRotation);

        // Assert
        Assert.That(adjustedRotationDelta, Is.EqualTo(expectedRotationDelta), message);
    }
}
