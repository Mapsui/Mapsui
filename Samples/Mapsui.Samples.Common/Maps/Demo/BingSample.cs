using BruTile.Cache;
using BruTile.Predefined;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class BingSample : ISample
{
    public string Name => "3 Virtual Earth";
    public string Category => "Demo";
    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap(BingArial.DefaultCache));
    }

    public static Map CreateMap(IPersistentCache<byte[]>? persistentCache, KnownTileSource source = KnownTileSource.BingAerial)
    {
        var map = new Map();

        var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
        map.Layers.Add(new TileLayer(KnownTileSources.Create(source, apiKey, persistentCache),
            dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "Bing Aerial",
        });
        map.Home = n => n.NavigateTo(new MPoint(1059114.80157058, 5179580.75916194), map.Resolutions[14]);
        map.BackColor = Color.FromString("#000613");

        return map;
    }
}
