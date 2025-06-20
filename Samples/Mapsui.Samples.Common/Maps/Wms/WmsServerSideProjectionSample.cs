using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Wms;

public class WmsServerSideProjectionSample : ISample
{
    public string Name => "WMS Server Side Projection";
    public string Category => "WMS";

    private const string _mapInfoLayerName = "Windspeed in ESPG:3857 (PDOK) ";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(await CreateLayerAsync());
        map.Navigator.CenterOnAndZoomTo(new MPoint(599458, 6852451), 1000);
        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _mapInfoLayerName));
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        return new ImageLayer(_mapInfoLayerName)
        {
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle(),
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://service.pdok.nl/rvo/windkaart/wms/v1_0?request=getcapabilities&service=wms";

        var provider = await WmsProvider.CreateAsync(wmsUrl);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:3857"; // Set the CRS on the provider to get the map in EPSG:3857

        provider.AddLayer("windsnelheden100m");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
