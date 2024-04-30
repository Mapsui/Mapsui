using Mapsui.Layers;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class HangingSample : ISample
{
    public string Name => "Hanging";
    public string Category => "1";

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateEmptyLayerWithMemoryProvider());
        return map;
    }

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    private static ILayer CreateEmptyLayerWithMemoryProvider() => new Layer("Layer")
    {
        DataSource = new Providers.MemoryProvider(),
    };
}
