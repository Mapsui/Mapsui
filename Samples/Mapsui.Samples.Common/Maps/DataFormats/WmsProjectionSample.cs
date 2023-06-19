using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using System.Threading.Tasks;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmsProjectionSample : ISample
{
    public string Name => " 6 WMS Projection";
    public string Category => "Data Formats";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Mapsui.Map
        {
            CRS = "EPSG:3857",
        };

        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        var dataSource = new ProjectingProvider(await CreateWmsProviderAsync())
        {
            CRS = "EPSG:3857"
        };

        return new ImageLayer("mainmap")
        {
            DataSource = dataSource,
            Style = new RasterStyle()
        };
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        const string wmsUrl = "https://sgi2.isprambiente.it/arcgis/services/raster/igm25k_lazio_wgs/ImageServer/WMSServer?service=wms&request=getCapabilities&version=1.3.0";
        var provider = await WmsProvider.CreateAsync(wmsUrl);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:4326";
        provider.AddLayer("igm25k_lazio_wgs");
        provider.SetImageFormat((provider.OutputFormats)[1]);
        return provider;
    }
}
