using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Desktop
{
    public static class SlowWmsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLayer());
            map.Home = n => n.NavigateTo(new Point(1031709.38634765, 7507541.80851409), 10);
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new ImageLayer("Windsnelheden (PDOK)") { DataSource = CreateWmsProvider() };
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl = "http://jordbrugsanalyser.dk/geoserver/ows?request=GetCapabilities&service=wms";
            var provider = new WmsProvider(wmsUrl)
            {
                ContinueOnError = true,
                TimeOut = 20000,
                CRS = "EPSG:3857"
            };

            provider.AddLayer("Jordbrugsanalyser:Marker12");
            provider.SetImageFormat(provider.OutputFormats[0]);
            return provider;
        }
    }
}