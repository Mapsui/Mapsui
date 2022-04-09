using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WmsSample : AsyncSampleBase
    {
        public override string Name => "6. WMS";
        public override string Category => "Data";
        public static IUrlPersistentCache? DefaultCache { get; set; }

        public override async Task SetupAsync(IMapControl mapControl)
        {
            mapControl.Map = await CreateMapAsync();
        }

        public static async Task<Map> CreateMapAsync()
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

            var provider = new WmsProvider(wmsUrl, persistentCache: DefaultCache)
            {
                ContinueOnError = true,
                TimeOut = 20000,
                CRS = "EPSG:28992"
            };

            provider.AddLayer("windsnelheden100m");
            await provider.SetImageFormatAsync((await provider.OutputFormatsAsync())[0]);
            return provider;
        }
    }
}