using System.IO;
using System.Linq;
using Mapsui.Extensions.Projections;
using Mapsui.Nts;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
namespace Mapsui.Tests.Projections;

[TestFixture]
public class ProjectionTests
{
    private WKTReader _wktReader = new WKTReader();

    [Test]
    public void MultiPolygonCoordinatesTest()
    {
        // arrange
        var geometry = _wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
        const int expectedCoordinateCount = 14;

        // act
        var enumeration = geometry.Coordinates;

        // assert
        Assert.That(enumeration.Count(), Is.EqualTo(expectedCoordinateCount));
    }

    [Test]
    public void MultiLineStringCoordinatesTest()
    {
        // arrange
        var geometry = _wktReader.Read("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");
        const int expectedCoordinateCount = 7;

        // act
        var enumeration = geometry.Coordinates;

        // assert
        Assert.That(enumeration.Count(), Is.EqualTo(expectedCoordinateCount));
    }

    [TestCase("EPSG:3857")]
    [TestCase("EPSG:3395")]
    public void CoordinateProjectionTest(string crs)
    {
        // arrange
        var multiPolygon = (MultiPolygon)_wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
        var projectedMultiPolygon = multiPolygon.Copy();
        var feature = new GeometryFeature(projectedMultiPolygon);
        var projection = new Projection();

        // act
        projection.Project("EPSG:4326", crs, feature);

        // assert
        var coordinates = multiPolygon.Coordinates.ToList();
        var projectedCoordinates = projectedMultiPolygon.Coordinates.ToList();

        for (var i = 0; i < coordinates.Count; i++)
        {
            Assert.That(projectedCoordinates[i].X, Is.Not.EqualTo(coordinates[i].X));
            Assert.That(projectedCoordinates[i].Y, Is.Not.EqualTo(coordinates[i].Y));
        }
    }

    [TestCase("EPSG:3857")]
    [TestCase("EPSG:3395")]
    public void CoordinateNtsProjectionTest(string crs)
    {
        // arrange
        var multiPolygon = (MultiPolygon)_wktReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
        var projectedMultiPolygon = multiPolygon.Copy();
        var feature = new GeometryFeature(projectedMultiPolygon);
        var projection = new DotSpatialProjection();

        // act
        projection.Project("EPSG:4326", crs, feature);

        // assert
        var coordinates = multiPolygon.Coordinates.ToList();
        var projectedCoordinates = projectedMultiPolygon.Coordinates.ToList();

        for (var i = 0; i < coordinates.Count; i++)
        {
            Assert.That(projectedCoordinates[i].X, Is.Not.EqualTo(coordinates[i].X));
            Assert.That(projectedCoordinates[i].Y, Is.Not.EqualTo(coordinates[i].Y));
        }
    }

    [Test]
    public void ShapeFileReadTest()
    {
        // arrange
        var directory = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        var countriesPath = Path.Combine(directory!, "GeoData", "World", "countries.shp");

        // act
        using var shapeFile = new ShapeFile(countriesPath, false, true, new DotSpatialProjection());

        // assert
        Assert.That("EPSG:4326", Is.EqualTo(shapeFile.CRS));
    }
}
