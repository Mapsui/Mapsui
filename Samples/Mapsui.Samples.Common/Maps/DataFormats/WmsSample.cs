using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmsSample : ISample, ISampleTest
{
    public string Name => " 6 WMS";
    public string Category => "Data Formats";
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
        return new ImageLayer("Windsnelheden (PDOK)") 
        { 
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle() 
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://geodata.nationaalgeoregister.nl/windkaart/wms?request=GetCapabilities";

        var provider = await WmsProvider.CreateAsync(wmsUrl, persistentCache: DefaultCache);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:28992";

        provider.AddLayer("windsnelheden100m");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }

    public Task InitializeTestAsync(IMapControl mapControl)
    {
        var extent = mapControl.Map?.Extent;
        if (extent != null)
        {
            if (mapControl.Viewport is IViewport viewport)
            {
                // Set Extend from Map
                var resolution = mapControl.Viewport.Resolution;
                viewport.SetCenter(extent.Centroid.X / resolution, extent.Centroid.Y / resolution);
                viewport.SetSize(extent.Width / resolution, extent.Width / resolution * (600.0 / 800.0)); // keep aspect ratio
            }
        }

        return Task.CompletedTask;
    }
}
