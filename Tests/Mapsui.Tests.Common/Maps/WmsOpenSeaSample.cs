using System.Collections.Generic;
using System.Threading.Tasks;
using BruTile;
using BruTile.Wmts.Generated;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Projections;
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
        var panBounds = WmsBasilicataSample.GetLimitsOfBasilicata();
        map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
        map.Navigator.RotationLock = true;
        map.Navigator.OverridePanBounds = panBounds;
        map.Home = n => n.ZoomToBox(panBounds);     
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

        var provider = await WmsProvider.CreateAsync(wmsUrl, userAgent: "Wms Basilicata Sample");
        provider.ContinueOnError = true;
        provider.TimeOut = 40000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("gebco_2021");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
