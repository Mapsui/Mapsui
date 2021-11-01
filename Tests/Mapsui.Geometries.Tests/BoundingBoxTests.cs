using NUnit.Framework;

namespace Mapsui.Geometries.Tests
{
    [TestFixture]
    public class BoundingBoxTests
    {
        [Test]
        public void BoundingBoxTouches()
        {
            // Arrange 
            var b1 = new BoundingBox(0, 0, 1, 1);
            var b2 = new BoundingBox(0, 1, 1, 2);

            // Act
            var touch = b1.Touches(b2);

            // Assert
            Assert.AreEqual(true, touch);
        }
    }
}
