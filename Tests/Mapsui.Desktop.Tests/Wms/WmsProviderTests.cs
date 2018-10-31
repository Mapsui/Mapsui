using System.Linq;
using System.Xml;
using Mapsui.Desktop.Tests.Utilities;
using Mapsui.Desktop.Wms;
using NUnit.Framework;

namespace Mapsui.Desktop.Tests.Wms
{
    [TestFixture]
    class WmsProviderTests
    {
        [Test]
        public void GetLegendRequestUrls_WhenInitialized_ShouldReturnListOfUrls()
        {
            // arrange
            var capabilties = new XmlDocument { XmlResolver = null };
            capabilties.Load($"{AssemblyInfo.AssemblyDirectory}\\Resources\\capabilities_1_3_0.xml");
            var provider = new WmsProvider(capabilties) { CRS = "EPSG:3857" };
            provider.AddLayer("Maasluis complex - top");
            provider.AddLayer("Kreftenheye z2 - top");
            provider.SetImageFormat(provider.OutputFormats[0]);
            provider.ContinueOnError = true;

            // act
            var legendUrls = provider.GetLegendRequestUrls();

            // assert
            Assert.True(legendUrls.Count() == 2);
        }
    }
}
