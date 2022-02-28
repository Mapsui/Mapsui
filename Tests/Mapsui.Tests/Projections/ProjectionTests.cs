using System.Linq;
using Mapsui.Nts;
using Mapsui.Nts.Projections;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace Mapsui.Tests.Projections
{
    [TestFixture]
    public class ProjectionTests
    {
        private WKTReader _wktReader = new WKTReader();

        [Test]
        public void MultiPolygonCoordinatesTest()
        {
            // arrange
            var geomety = _wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            const int expectedCoordinateCount = 14;

            // act
            var enumeration = geomety.Coordinates;

            // assert
            Assert.AreEqual(expectedCoordinateCount, enumeration.Count());
        }

        [Test]
        public void MultiLineStringCoordinatesTest()
        {
            // arrange
            var geomety = _wktReader.Read("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");
            const int expectedCoordinateCount = 7;

            // act
            var enumeration = geomety.Coordinates;

            // assert
            Assert.AreEqual(expectedCoordinateCount, enumeration.Count());
        }

        [Test]
        public void CoordinateProjectionTest()
        {
            // arrange
            var multiPolygon = (MultiPolygon)_wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            var projectedMultiPolygon = multiPolygon.Copy();
            using var feature = new GeometryFeature(projectedMultiPolygon);
            var projection = new Projection();

            // act
            projection.Project("EPSG:4326", "EPSG:3857", feature);

            // assert
            var coordinates = multiPolygon.Coordinates.ToList();
            var projectedCoordinates = projectedMultiPolygon.Coordinates.ToList();

            for (var i = 0; i < coordinates.Count; i++)
            {
                Assert.AreNotEqual(coordinates[i].X, projectedCoordinates[i].X);
                Assert.AreNotEqual(coordinates[i].Y, projectedCoordinates[i].Y);
            }
        }

        [Test]
        public void CoordinateNtsProjectionTest()
        {
            // arrange
            var multiPolygon = (MultiPolygon)_wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            var projectedMultiPolygon = multiPolygon.Copy();
            using var feature = new GeometryFeature(projectedMultiPolygon);
            var projection = new DotSpatialProjection();

            // act
            projection.Project("EPSG:4326", "EPSG:3857", feature);

            // assert
            var coordinates = multiPolygon.Coordinates.ToList();
            var projectedCoordinates = projectedMultiPolygon.Coordinates.ToList();

            for (var i = 0; i < coordinates.Count; i++)
            {
                Assert.AreNotEqual(coordinates[i].X, projectedCoordinates[i].X);
                Assert.AreNotEqual(coordinates[i].Y, projectedCoordinates[i].Y);
            }
        }
    }
}
