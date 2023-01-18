using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using BruTile.Wmsc;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

/// <summary>
/// An ordinary WMS service called through a tiled schema (WMS-C) 
/// </summary>
public class TiledWmsSample : ISample
{
    public string Name => " 4 WMS called tiled";
    public string Category => "Data Formats";
    public static IPersistentCache<byte[]>? DefaultCache { get; set; }

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(CreateLayer());
        return Task.FromResult(map);
    }

    public static ILayer CreateLayer()
    {
        return new TileLayer(CreateTileSource()) { Name = "Omgevingswarmte (PDOK)" };
    }

    public static ITileSource CreateTileSource()
    {
        const string url = "https://geodata.nationaalgeoregister.nl/omgevingswarmte/wms?SERVICE=WMS&VERSION=1.1.1";
        // You need to know the schema. This can be a problem. Usally it is GlobalSphericalMercator
        var schema = new WkstNederlandSchema { Format = "image/png", Srs = "EPSG:28992" };
        var request = new WmscRequest(new Uri(url), schema, new[] { "koudegeslotenwkobuurt" }.ToList(), Array.Empty<string>().ToList());
        var provider = new HttpTileProvider(request, DefaultCache);
        return new TileSource(provider, schema) { Name = "Omgevingswarmte (PDOK)" };
    }
}
