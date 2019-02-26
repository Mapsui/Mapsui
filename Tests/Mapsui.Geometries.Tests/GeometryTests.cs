using System.Collections.Generic;
using NUnit.Framework;

namespace Mapsui.Geometries.Tests
{
    [TestFixture]
    public class GeometryTests
    {
        [Test]
        public void TestPointEquals()
        {
            // arrange
            var point = new Point(0, 42);
            var pointSame = new Point(0, 42);
            var pointDifferent = new Point(42, 1337);
            Geometry geometry = pointSame;

            // assert
            Assert.IsTrue(point.Equals(pointSame));
            Assert.IsFalse(point.Equals(pointDifferent));
            Assert.IsTrue(point.Equals(geometry));
        }

        [Test]
        public void TestLineStringEquals()
        {
            // arrange
            var lineString = new LineString(new[] {new Point(0, 42), new Point(42, 1337)});
            var lineStringSame = new LineString(new[] { new Point(0, 42), new Point(42, 1337) });
            var lineStringDifferent = new LineString(new[] { new Point(42, 1337), new Point(0, 42) });
            Geometry geometry = lineStringSame;
            
            // assert
            Assert.IsTrue(lineString.Equals(lineStringSame));
            Assert.IsFalse(lineString.Equals(lineStringDifferent));
            Assert.IsTrue(lineString.Equals(geometry));
        }

        [Test]
        public void TestPolygonEquals()
        {
            // arrange
            var point1 = new Point(0, 42);
            var point2 = new Point(42, 1337);
            var point3 = new Point(1337, 0);

            var polygon = new Polygon(new LinearRing(new List<Point> { point1, point2, point3 }));
            var polygonSame = new Polygon(new LinearRing(new List<Point> { point1, point2, point3 }));
            var polygonDifferent = new Polygon(new LinearRing(new List<Point> { point1, point3, point2 }));
            Geometry geometry = polygonSame;

            // assert
            Assert.IsFalse(polygon.Equals(point1));
            Assert.IsTrue(polygon.Equals(polygonSame));
            Assert.IsFalse(polygon.Equals(polygonDifferent));
            Assert.IsTrue(polygon.Equals(geometry));
        }
    }
}
