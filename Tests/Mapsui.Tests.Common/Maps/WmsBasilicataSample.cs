using System.Collections.Generic;
using System.Threading.Tasks;
using BruTile;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers.Wms;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tests.Common.Maps;

public class WmsBasilicataSample : ISample
{
    public string Name => "Wms Basilicata";
    public string Category => "Tests";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map { CRS = "EPSG:4326" };
        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        map.Home = (n) => n.CenterOnAndZoomTo(SphericalMercator.FromLonLat(15.804827, 40.63506).ToMPoint(), 500);
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

        var provider = await WmsProvider.CreateAsync(wmsUrl);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:4326";

        provider.AddLayer("LC.LandCoverRaster");
        provider.SetImageFormat(provider.OutputFormats[0]);
        return provider;
    }
}
