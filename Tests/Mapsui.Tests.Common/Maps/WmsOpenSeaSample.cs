using System.Collections.Generic;
using System.Threading.Tasks;
using BruTile;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tests.Common.Maps;

public class WmsOpenSeaSample : ISample
{
    public string Name => "Wms OpenSea";
    public string Category => "Tests";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:4326" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        map.Home = (n) => n.CenterOnAndZoomTo(new MPoint(155000, 463000), 500);
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        return new ImageLayer("Opensea")
        {
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle()
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://depth.openseamap.org/geoserver/gebco2021/wms";

        var provider = await WmsProvider.CreateAsync(wmsUrl);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("gebco_2021");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
