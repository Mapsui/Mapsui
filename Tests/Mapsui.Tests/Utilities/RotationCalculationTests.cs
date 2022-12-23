using Mapsui.Utilities;
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
        var rotation = RotationCalculations.NormalizeRotation(inputRotation);

        // Assert
        Assert.AreEqual(expectedRotation, rotation);
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
        var distance = RotationCalculations.RotationShortestDistance(inputRotation1, inputRotation2);

        // Assert
        Assert.AreEqual(expectedDistance, distance);
    }

    [TestCase(15, 0, 20, 10, 0)] // Still snapped
    [TestCase(-15, 0, 20, 10, 0)] // Still snapped
    [TestCase(25, 0, 20, 10, 25)] // Unsnap
    [TestCase(-25, 0, 20, 10, -25)] // Unsnap
    [TestCase(30, 25, 20, 10, 5)] // Still unsnapped
    [TestCase(-30, -25, 20, 10, -5)] // Still unsnapped
    [TestCase(5, 15, 20, 10, -15)] // Resnap
    [TestCase(-5, -15, 20, 10, 15)] // Resnap

    public static void TestCalculateRotationDeltaUsingSnapping(double virtualRotation, double actualRotation, double unSnapRotation, double reSnapRotation, double expectedRotationDelta)
    {
        // Act
        var rotationDelta = RotationCalculations.CalculateRotationDeltaWithSnapping(virtualRotation, actualRotation, unSnapRotation, reSnapRotation);

        // Assert
        Assert.AreEqual(expectedRotationDelta, rotationDelta);
    }   
}
