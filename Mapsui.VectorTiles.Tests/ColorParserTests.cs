using NUnit.Framework;

namespace Mapsui.VectorTiles.Tests
{
    [TestFixture]
    public class ColorParserTests
    {
        [Test]
        public void TestHslToRgb()
        {
            // arrange 
            var hsl = "hsl(205, 56%, 73%)";

            // act
            var color = ColorParser.HslToColor(hsl);

            // assert
            //rgba(148, 193, 225, 1)
            Assert.AreEqual(color.R, 148);
            Assert.AreEqual(color.G, 193);
            Assert.AreEqual(color.B, 225);

        }
    }
}
