using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Providers.Wms;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.WMS;

public class WmsOpenSeaSample : ISample
{
    public string Name => "WMS OpenSea";
    public string Category => "WMS";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:4326" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        var panBounds = GetLimitsOfItaly();
        map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
        map.Navigator.RotationLock = true;
        map.Navigator.OverridePanBounds = panBounds;
        map.Navigator.ZoomToBox(panBounds);

        return map;
    }

    private static MRect GetLimitsOfItaly()
    {
        var (minX, minY) = (6.7499552751, 36.619987291);
        var (maxX, maxY) = (18.4802470232, 47.1153931748);
        return new MRect(minX, minY, maxX, maxY);
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
        const string wmsUrl = "https://depth.openseamap.org/geoserver/ows";

        var provider = await WmsProvider.CreateAsync(wmsUrl, userAgent: "Wms OpenSea Sample", wmsVersion: "1.3.0");
        provider.ContinueOnError = true;
        provider.TimeOut = 40000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("openseamap:contour2");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
