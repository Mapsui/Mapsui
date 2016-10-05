using Mapsui.Layers;
using Mapsui.Providers.Wms;

namespace Mapsui.Samples.Common.Desktop
{
    public static class WmsSample
    {
        public static Map CreateMap()
        {
            var map = new Map {CRS = "EPSG:28992"};
            // The WMS request needs a CRS
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new ImageLayer("WMS Layer") {DataSource = CreateWmsProvider()};
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl =
                "http://geodata.nationaalgeoregister.nl/ahn25m/wms?service=wms&request=getcapabilities";

            var provider = new WmsProvider(wmsUrl)
            {
                ContinueOnError = true,
                TimeOut = 20000,
                CRS = "EPSG:28992"
            };

            provider.AddLayer("ahn25m");
            provider.SetImageFormat(provider.OutputFormats[0]);
            return provider;
        }
    }
}