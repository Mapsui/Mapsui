using HaversineFormula;
using Mapsui.Projection;
using NUnit.Framework;

namespace Mapsui.Tests.Projection
{
    [TestFixture]
    internal class SphericalMercatorTests
    {
        [Test]
        public void SphericalMercatorPrecisionTest()
        {
            // Arrange
            double inLon = 101.71046179910427;
            double inLat = 3.1567427968766819;

            // Act
            var (x, y) = SphericalMercator.FromLonLat(inLon, inLat);
            var (outLon, outLat) = SphericalMercator.ToLonLat(x, y);

            // Assert
            var distanceInKilometer = Haversine.Distance(inLon, inLat, outLon, outLat);
            var distanceInCentimer = distanceInKilometer * 100000;
            Assert.Less(distanceInCentimer, 1);
        }
    }
}
