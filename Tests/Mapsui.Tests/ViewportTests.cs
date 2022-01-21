using NUnit.Framework;

namespace Mapsui.Tests
{
    [TestFixture]
    public class ViewportTests
    {
        [Test]
        public void SetCenterTest()
        {
            // Arrange
            var viewport = new Viewport();

            // Act
            viewport.SetCenter(10, 20);

            // Assert
            Assert.AreEqual(10, viewport.CenterX);
            Assert.AreEqual(20, viewport.CenterY);
        }
    }
}
