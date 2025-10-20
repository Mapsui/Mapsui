using BruTile.Cache;
using BruTile.Wmts;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmtsSample : ISample
{
    public string Name => "WMTS";
    public string Category => "DataFormats";
    public static IPersistentCache<byte[]>? DefaultCache { get; set; }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:28992"
        };
        map.Layers.Add(await CreateLayerAsync());
        map.Layers.Add(CreateGeodanOfficesLayer());
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

    private static MemoryLayer CreateGeodanOfficesLayer()
    {
        var geodanAmsterdam = new MPoint(122698, 483922);
        var geodanDenBosch = new MPoint(148949, 411446);
        var imageSource = "embedded://Mapsui.Samples.Common.Images.location.png";

        var layer = new MemoryLayer
        {
            Features = new[] { geodanAmsterdam, geodanDenBosch }.ToFeatures(),
            Style = new ImageStyle
            {
                Image = imageSource,
                Offset = new Offset { Y = 64 },
                SymbolScale = 0.25
            },
            Name = "Geodan Offices"
        };
        return layer;
    }
}
