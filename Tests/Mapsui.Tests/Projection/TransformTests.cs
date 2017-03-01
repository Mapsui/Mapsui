using Mapsui.Projection;
using NUnit.Framework;
using System.Linq;
using Mapsui.Geometries.WellKnownText;

namespace Mapsui.Tests.Projection
{
    [TestFixture]
    public class TransformTests
    {
        [Test]
        public void MultiPolygonAllVerticesTest()
        {
            // arrange
            var geomety = GeometryFromWKT.Parse("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            const int numberOfVectices = 14;

            // act
            var enumeration = geomety.AllVertices();

            // assert
            Assert.AreEqual(numberOfVectices, enumeration.Count());
        }

        [Test]
        public void MultiLineStringAllVerticesTest()
        {
            // arrange
            var geomety = GeometryFromWKT.Parse("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");
            const int numberOfVectices = 7;

            // act
            var enumeration = geomety.AllVertices();

            // assert
            Assert.AreEqual(numberOfVectices, enumeration.Count());
        }

        [Test]
        public void AllVerticesTransformTest()
        {
            // arrange
            var geomety = GeometryFromWKT.Parse("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            var transformation = new MinimalTransformation();

            // act
            var enumeration = transformation.Transform("EPSG:4326", "EPSG:3857", geomety);

            // assert
            Assert.AreEqual(14, enumeration.AllVertices().Count());
        }
    }
}
