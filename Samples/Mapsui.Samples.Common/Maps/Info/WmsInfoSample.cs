using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.Common.Maps.Info;

public class WmsInfoSample : ISample
{
    public string Name => "4 Wms Info";
    public string Category => "Info";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:28992" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        map.Navigator.CenterOnAndZoomTo(new MPoint(155000, 463000), 500);
        map.Widgets.Add(new MapInfoWidget(map));
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        return new ImageLayer("Windsnelheden (PDOK)")
        {
            DataSource = await CreateWmsProviderAsync(),
            Style = new RasterStyle(),
            IsMapInfoLayer = true,
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://service.pdok.nl/rvo/windkaart/wms/v1_0?request=getcapabilities&service=wms";

        var provider = await WmsProvider.CreateAsync(wmsUrl);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:28992";

        provider.AddLayer("windsnelheden100m");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
