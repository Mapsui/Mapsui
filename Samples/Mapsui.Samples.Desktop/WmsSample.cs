using Mapsui.Layers;
using Mapsui.Providers.Wms;

namespace Mapsui.Samples.Desktop
{
    public static class WmsSample
    {
        public static ILayer Create()
        {
            var provider = CreateWmsProvider();
            var layer = new ImageLayer("WmsLayer") {DataSource = provider};
            return layer;
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";

            var provider = new WmsProvider(wmsUrl)
                {
                    SpatialReferenceSystem = "EPSG:900913",
                    ContinueOnError = true,
                    TimeOut = 20000,
                    SRID = 900913
                };

            provider.AddLayer("World");
            provider.SetImageFormat(provider.OutputFormats[0]);
            return provider;
        }
    }
}