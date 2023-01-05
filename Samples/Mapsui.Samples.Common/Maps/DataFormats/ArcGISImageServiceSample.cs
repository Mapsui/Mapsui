using Mapsui.ArcGIS;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.ArcGIS.ImageServiceProvider;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Styles;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class ArcGISImageServiceSample : ISample
{
    private const string LandsatGlsImageServer = @"https://landsat2.arcgis.com/arcgis/rest/services/LandsatGLS/MS/ImageServer";
    private ArcGISImageCapabilities? _capabilities;

    public string Name => "11 ArcGIS image";
    public string Category => "Data Formats";

    public static IUrlPersistentCache? DefaultCache { get; set; }

    public async Task<ILayer> CreateLayerAsync()
    {
        var layer = new ImageLayer("ArcGISImageServiceLayer")
        {
            DataSource = await CreateProviderAsync()
        };

        var arcGisLegend = new ArcGisLegend(DefaultCache);
        var legend = await arcGisLegend.GetLegendInfoAsync(LandsatGlsImageServer);
#pragma warning disable CS8602
        layer.Name = legend.layers[0].layerName ?? "ArcGisImage";
#pragma warning restore CS8602            

        layer.Style = new RasterStyle();
        return layer;
    }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            Home = n => n.NavigateTo(new MPoint(1270000.0, 5880000.0), 10000)
        };
        map.Layers.Add(await CreateLayerAsync());
        return map;
    }

    private async Task<ArcGISImageServiceProvider> CreateProviderAsync()
    {
        // https://landsat2.arcgis.com/arcgis/rest/services/LandsatGLS/MS/ImageServer/exportImage?bbox=-2.00375070672E7%2C-8572530.6034%2C2.0037507842788246E7%2C1.68764993966E7&bboxSR=&size=&imageSR=&time=&format=jpgpng&pixelType=S16&noData=&noDataInterpretation=esriNoDataMatchAny&interpolation=+RSP_BilinearInterpolation&compression=&compressionQuality=&bandIds=&sliceId=&mosaicRule=&renderingRule=&adjustAspectRatio=true&validateExtent=false&lercVersion=1&compressionTolerance=&f=image
        var capabilitiesHelper = new CapabilitiesHelper(DefaultCache);
        capabilitiesHelper.CapabilitiesReceived += CapabilitiesReceived;
        capabilitiesHelper.CapabilitiesFailed += capabilitiesHelper_CapabilitiesFailed;
        capabilitiesHelper.GetCapabilities(LandsatGlsImageServer, CapabilitiesType.ImageServiceCapabilities);

        while (_capabilities == null)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        return new ArcGISImageServiceProvider(_capabilities, persistentCache: DefaultCache);
    }

    private static void capabilitiesHelper_CapabilitiesFailed(object? sender, System.EventArgs e)
    {
        Logger.Log(LogLevel.Warning, "ArcGISImageService capabilities request failed");
    }

    private void CapabilitiesReceived(object? sender, System.EventArgs e)
    {
        _capabilities = sender as ArcGISImageCapabilities;
    }
}
