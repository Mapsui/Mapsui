using BruTile.Cache;
using BruTile.Wmts;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmtsSample : ISample
{
    public string Name => "WMTS";
    public string Category => "Data Formats";
    public static IPersistentCache<byte[]>? DefaultCache { get; set; }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:28992"
        };
        map.Layers.Add(await CreateLayerAsync());
        map.Layers.Add(GeodanOfficesLayerBuilder.Create());
        map.Navigator.CenterOnAndZoomTo(new MPoint(155000, 463000), 500);
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        var url = "https://service.pdok.nl/brt/achtergrondkaart/wmts/v2_0?request=GetCapabilities&service=wmts";

        var bytes = await (DefaultCache as IUrlPersistentCache).GetCachedBytesAsync(url);
        using var stream = new MemoryStream(bytes);
        var tileSources = WmtsCapabilitiesParser.Parse(stream);
        var nature2000TileSource = tileSources.FirstOrDefault(t => t.Name == "top1000raster") ?? tileSources.First();
        if (DefaultCache != null)
        {
            nature2000TileSource.PersistentCache = DefaultCache;
        }

        return new TileLayer(nature2000TileSource) { Name = nature2000TileSource.Name };

    }
}
