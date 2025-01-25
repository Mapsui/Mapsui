using Mapsui.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Utilities;

[TestFixture]
public class HaversineTests
{
    [Test]
    public void Distance_TwoPoints_ReturnsCorrectDistance()
    {
        // Arrange
        double lon1 = 0;
        double lat1 = 0;
        double lon2 = 0;
        double lat2 = 1;

        // Act
        double result = Haversine.Distance(lon1, lat1, lon2, lat2);

        // Assert
        Assert.That(result, Is.EqualTo(111.19).Within(0.01));
    }

    [Test]
    public void Distance_MultiplePoints_ReturnsCorrectTotalDistance()
    {
        // Arrange
        var points = new MPoint[]
        {
            new(0, 0),
            new(0, 1),
            new(1, 1)
        };

        // Act
        double result = Haversine.Distance(points);

        // Assert
        Assert.That(result, Is.EqualTo(222.39).Within(0.02));
    }

    [Test]
    public void Distance_SamePoint_ReturnsZero()
    {
        // Arrange
        double lon1 = 0;
        double lat1 = 0;
        double lon2 = 0;
        double lat2 = 0;

        // Act
        double result = Haversine.Distance(lon1, lat1, lon2, lat2);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }
}
