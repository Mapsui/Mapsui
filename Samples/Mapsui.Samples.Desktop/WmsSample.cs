using System;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.Styles;

namespace Mapsui.Samples
{
    public static class WmsSample
    {
        public static ILayer Create()
        {
            var provider = CreateWmsProvider();
            var layer = new ImageLayer("WmsLayer");
            layer.DataSource = provider;
            layer.DataSource.SRID = 900913;
            return layer;
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";

            var provider = new WmsProvider(wmsUrl);
            provider.SpatialReferenceSystem = "EPSG:900913";
            provider.AddLayer("World");
            provider.SetImageFormat(provider.OutputFormats[0]);
            provider.ContinueOnError = true;
            provider.TimeOut = 20000; //Set timeout to 20 seconds
            return provider;
        }
    }
}