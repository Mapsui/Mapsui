using Mapsui.Projections;
using Mapsui.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Projections;

[TestFixture]
internal class MercatorTests
{
    [Test]
    public void MercatorPrecisionTest()
    {
        // Arrange
        var inLon = 101.71046179910427;
        var inLat = 3.1567427968766819;

        // Act
        var (x, y) = Mercator.FromLonLat(inLon, inLat);
        var (outLon, outLat) = Mercator.ToLonLat(x, y);

        // Assert
        var distanceInKilometer = Haversine.Distance(inLon, inLat, outLon, outLat);
        var distanceInCentimeter = distanceInKilometer * 100000;
        ClassicAssert.Less(distanceInCentimeter, 1);
    }
}
