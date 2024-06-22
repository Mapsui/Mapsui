using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Providers.Wms;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.WMS;

public class WmsGebcoSample : ISample
{
    public string Name => "WMS Gebco";
    public string Category => "WMS";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:4326" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        var panBounds = WmsBasilicataSample.GetLimitsOfBasilicata();
        map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
        map.Navigator.RotationLock = true;
        map.Navigator.OverridePanBounds = panBounds;
        map.Navigator.ZoomToBox(panBounds);

        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        return new ImageLayer("Gebco")
        {
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle()
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://wms.gebco.net/2021/mapserv";

        var provider = await WmsProvider.CreateAsync(wmsUrl, userAgent: "Wms Gebco Sample", wmsVersion: "1.3.0");
        provider.ContinueOnError = true;
        provider.TimeOut = 40000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("GEBCO_2021");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
