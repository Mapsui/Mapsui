using NUnit.Framework;

namespace Mapsui.Tests;

[TestFixture]
public class MRectTests
{
    [TestCase(2, 2, 8, 8, true)]
    [TestCase(5, 5, 15, 15, true)]
    [TestCase(-5, -5, 5, 5, true)]
    [TestCase(5, -5, 15, 5, true)]
    [TestCase(-5, 5, 5, 15, true)]
    [TestCase(0, -10, 10, 0, false)]
    [TestCase(0, 10, 10, 20, false)]
    [TestCase(-10, 0, 0, 10, false)]
    [TestCase(10, 0, 20, 10, false)]
    public void IntersectsTest(
        double minX, double minY, double maxX, double maxY,
        bool intersect)
    {
        // Arrange
        var rect1 = new MRect(0, 0, 10, 10);
        var rect2 = new MRect(minX, minY, maxX, maxY);

        // Act
        var result = rect1.Intersects(rect2);

        // Assert
        Assert.AreEqual(intersect, result);
    }
}
