using System;
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
        if (mapControl.Viewport is IViewport viewport)
        {
            // Set Center to Visible Map
            viewport.SetCenter(412, 1316);
        }

        return Task.CompletedTask;
    }
}
