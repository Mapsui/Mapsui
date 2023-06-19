using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using System.Threading.Tasks;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmsSample : ISample
{
    public string Name => " 6 WMS";
    public string Category => "Data Formats";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:28992" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        map.Home = (n) => n.CenterOnAndZoomTo(new MPoint(155000, 463000), 500);
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
