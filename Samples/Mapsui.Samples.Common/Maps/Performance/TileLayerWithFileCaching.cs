using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Performance;
public class TileLayerWithFileCaching : ISample
{     
    public string Name => "TileLayer File Caching";
    public string Category => "Performance";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "<Pending>")]
    public static Map CreateMap()
    {
        var map = new Map();

        var folder = (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        IPersistentCache<byte[]> tileCache = new FileCache(Path.Combine(folder, "tilecache"), "png");
        var httpLayer = new TileLayer(CreateTileSource("my-user-agent", tileCache));
        map.Layers.Add(httpLayer);
        return map;
    }

    private static HttpTileSource CreateTileSource(string userAgent, IPersistentCache<byte[]> tileCache)
    {
        return new HttpTileSource(new GlobalSphericalMercator(),
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            new[] { "a", "b", "c" }, name: "OpenStreetMap",
            attribution: OpenStreetMapAttribution, userAgent: userAgent, persistentCache: tileCache);
    }

    private static readonly BruTile.Attribution OpenStreetMapAttribution = new(
    "© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright");

}
