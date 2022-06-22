using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WmsSample : ISample
    {
        public string Name => "6. WMS";
        public string Category => "Data";
        public static IUrlPersistentCache? DefaultCache { get; set; }

        public async Task<Map> CreateMapAsync()
        {
            var map = new Map { CRS = "EPSG:28992" };
            // The WMS request needs a CRS
            map.Layers.Add(await CreateLayerAsync());
            return map;
        }

        public static async Task<ILayer> CreateLayerAsync()
        {
            return new ImageLayer("Windsnelheden (PDOK)") { DataSource = await CreateWmsProviderAsync() };
        }

        private static async Task<WmsProvider> CreateWmsProviderAsync()
        {
            const string wmsUrl = "https://geodata.nationaalgeoregister.nl/windkaart/wms?request=GetCapabilities";

            var provider = await WmsProvider.CreateAsync(wmsUrl, persistentCache: DefaultCache);
            provider.ContinueOnError = true;
            provider.TimeOut = 20000;
            provider.CRS = "EPSG:28992";

            provider.AddLayer("windsnelheden100m");
            provider.SetImageFormat((provider.OutputFormats)[0]);
            return provider;
        }
    }
}