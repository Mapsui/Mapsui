using System;
using System.Linq;
using NUnit.Framework;
using SharpMap.Providers.Wms;

namespace SharpMap.Providers.Tests.Wms
{
    [TestFixture]
    class WmsProviderTests
    {
        [Test]
        public void WmsGetLegend_WhenExists_ShouldReturnProperly()
        {
            const string wmsUrl = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";

            var provider = new WmsProvider(wmsUrl);
            provider.SpatialReferenceSystem = "EPSG:900913";
            provider.AddLayer("World");
            provider.SetImageFormat(provider.OutputFormats[0]);
            provider.ContinueOnError = true;
            provider.TimeOut = 20000; //Set timeout to 20 seconds
        }
    }
}
