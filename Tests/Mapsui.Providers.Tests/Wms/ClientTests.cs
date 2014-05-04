using System;
using System.Linq;
using NUnit.Framework;
using Mapsui.Web.Wms;
using System.Xml;

namespace Mapsui.Providers.Tests.Wms
{
    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void ParseCapabilities_WhenInputIsWellFormattedWms111_ShouldParseWithoutExceptions()
        {
            // arrange
            var capabilties = new XmlDocument { XmlResolver = null };
            capabilties.Load(".\\Resources\\capabilities_1_1_1.xml");

            // act
            var client = new Client(capabilties);

            // assert
            Assert.True(client != null);
        }

        [Test]
        public void ParseCapabilities_WhenInputIsWellFormattedWms130_ShouldParseWithoutExceptions()
        {
            // arrange
            var capabilties = new XmlDocument();
            capabilties.XmlResolver = null;
            capabilties.Load(".\\Resources\\capabilities_1_3_0.xml");

            // act
            var client = new Client(capabilties);

            // assert
            Assert.True(client != null);
        }
    }
}
