using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Limiting;
using Mapsui.Providers.Wms;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.WMS;

public class WmsBasilicataSample : ISample
{
    public string Name => "WMS Basilicata";
    public string Category => "WMS";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map() { CRS = "EPSG:4326" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        var panBounds = GetLimitsOfBasilicata();
        map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
        map.Navigator.RotationLock = true;
        map.Navigator.OverridePanBounds = panBounds;
        map.Navigator.ZoomToBox(panBounds);

        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        return new ImageLayer("Basilicata")
        {
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle()
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "http://rsdi.regione.basilicata.it:80/rbgeoserver2016/maps_ctr/LC.LandCoverRaster/ows";

        var provider = await WmsProvider.CreateAsync(wmsUrl, userAgent: "Wms Basilicata Sample");
        provider.ContinueOnError = true;
        provider.TimeOut = 40000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("LC.LandCoverRaster");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }

    public static MRect GetLimitsOfBasilicata()
    {
        var (minX, minY) = (13.804827, 38.63506);
        var (maxX, maxY) = (17.804827, 42.63506);
        return new MRect(minX, minY, maxX, maxY);
    }
}
