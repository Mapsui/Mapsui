using System.IO;
using BruTile;
using BruTile.FileSystem;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling.Layers;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class MapTilerSample : ISample
{
    static MapTilerSample()
    {
        MapTilesDeployer.CopyEmbeddedResourceToFile("TrueMarble");
    }

    public string Name => "Tiles on file system";
    public string Category => "Data Formats";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(CreateLayer());
        return map;
    }

    public static ILayer CreateLayer()
        => new TileLayer(new FileTileSource(GetTileSchema(), Path.Combine(MapTilesDeployer.MapTileLocation, "TrueMarble"), "png", name: "MapTiler")) { Name = "True Marble in MapTiler" };

    private static GlobalSphericalMercator GetTileSchema()
    {
        var schema = new GlobalSphericalMercator(YAxis.TMS);
        schema.Resolutions.Clear();
        schema.Resolutions[0] = new Resolution(0, 156543.033900000);
        schema.Resolutions[1] = new Resolution(1, 78271.516950000);
        return schema;
    }
}
