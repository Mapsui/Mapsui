using Mapsui.Layers;
using Mapsui.Providers.Wms;

namespace Mapsui.Samples.Common.Desktop
{
    public static class WmsSample
    {
        public static ILayer Create()
        {
            var provider = CreateWmsProvider();
            var layer = new ImageLayer("WmsLayer")
            {
                DataSource = provider,
                CRS = "EPSG:28992"
            };
            return layer;
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl = "http://geodata.nationaalgeoregister.nl/ahn25m/wms?service=wms&request=getcapabilities";

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