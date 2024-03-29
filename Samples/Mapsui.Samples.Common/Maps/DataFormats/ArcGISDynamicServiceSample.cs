using Mapsui.ArcGIS;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class ArcGISDynamicServiceSample : ISample
{
    private const string SampleWorldCities = @"https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer";
    private ArcGISDynamicCapabilities? _capabilities;

    public string Name => "ArcGIS dynamic";
    public string Category => "Data Formats";

    public static IUrlPersistentCache? DefaultCache { get; set; }

    public async Task<ILayer> CreateLayerAsync()
    {
        var layer = new ImageLayer("ArcGISDynamicServiceLayer")
        {
            DataSource = await CreateProviderAsync(),
        };

        var arcGisLegend = new ArcGisLegend(DefaultCache);
        var legend = await arcGisLegend.GetLegendInfoAsync(SampleWorldCities);
#pragma warning disable CS8602
        layer.Name = legend.layers[0].layerName ?? "ArcGisImage";
#pragma warning restore CS8602            

        layer.Style = new RasterStyle();
        return layer;
    }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Navigator.CenterOnAndZoomTo(new MPoint(1270000.0, 5880000.0), 10000);

        map.Layers.Add(await CreateLayerAsync());

        return map;
    }

    private async Task<ArcGISDynamicProvider> CreateProviderAsync()
    {
        var capabilitiesHelper = new CapabilitiesHelper(DefaultCache);
        capabilitiesHelper.CapabilitiesReceived += CapabilitiesReceived;
        capabilitiesHelper.CapabilitiesFailed += capabilitiesHelper_CapabilitiesFailed;
        capabilitiesHelper.GetCapabilities(SampleWorldCities, CapabilitiesType.DynamicServiceCapabilities);

        while (_capabilities == null)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        return new ArcGISDynamicProvider(SampleWorldCities, _capabilities, persistentCache: DefaultCache)
        {
            CRS = "EPSG:3857",
        };
    }

    private static void capabilitiesHelper_CapabilitiesFailed(object? sender, System.EventArgs e)
    {
        Logger.Log(LogLevel.Warning, "ArcGISImageService capabilities request failed");
    }

    private void CapabilitiesReceived(object? sender, System.EventArgs e)
    {
        _capabilities = sender as ArcGISDynamicCapabilities;
    }
}
