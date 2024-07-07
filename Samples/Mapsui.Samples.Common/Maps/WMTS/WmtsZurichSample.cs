using System.Linq;
using System.Threading.Tasks;
using BruTile.Cache;
using BruTile.Wmts;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;

namespace Mapsui.Samples.Common.Maps.WMTS;

public class WmtsZurichSample : ISample
{
    public string Name => "WMTS Zurich";
    public string Category => "WMTS";
    public static IPersistentCache<byte[]>? DefaultCache { get; set; }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:2056"
        };
        map.Layers.Add(await CreateLayerAsync());
        map.Navigator.CenterOnAndZoomTo(new MPoint(2672155, 1251624), 500);
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        var url = " https://www.ogd.stadt-zuerich.ch/mapproxy/wmts/1.0.0/WMTSCapabilities.xml";

        using var response = await (DefaultCache as IUrlPersistentCache).UrlCachedStreamAsync(url);
        var tileSources = WmtsCapabilitiesParser.Parse(response);
        var stadtplanSource = tileSources.FirstOrDefault(t => t.Name == "Stadtplan") ?? tileSources.First();
        if (DefaultCache != null)
        {
            stadtplanSource.PersistentCache = DefaultCache;
        }

        return new TileLayer(stadtplanSource) { Name = stadtplanSource.Name };
    }
}
