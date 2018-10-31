using System.Xml;
using Mapsui.Desktop.Tests.Utilities;
using Mapsui.Desktop.Wms;
using NUnit.Framework;

namespace Mapsui.Desktop.Tests.Wms
{
    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void ParseCapabilities_WhenInputIsWellFormattedWms111_ShouldParseWithoutExceptions()
        {
            // arrange
            var capabilties = new XmlDocument { XmlResolver = null };
            capabilties.Load($"{AssemblyInfo.AssemblyDirectory}\\Resources\\capabilities_1_1_1.xml");

            // act
            var client = new Client(capabilties);

            // assert
            Assert.True(client != null);
        }

        [Test]
        public void ParseCapabilities_WhenInputIsWellFormattedWms130_ShouldParseWithoutExceptions()
        {
            // arrange
            var capabilties = new XmlDocument {XmlResolver = null};
            capabilties.Load($"{AssemblyInfo.AssemblyDirectory}\\Resources\\capabilities_1_3_0.xml");

            // act
            var client = new Client(capabilties);

            // assert
            Assert.True(client != null);
        }
    }
}
