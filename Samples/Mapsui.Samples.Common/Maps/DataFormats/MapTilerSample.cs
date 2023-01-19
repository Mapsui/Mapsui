using System.IO;
using BruTile;
using BruTile.Cache;
using BruTile.FileSystem;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using System.Threading.Tasks;
using Attribution = BruTile.Attribution;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class MapTilerSample : IMapControlSample
{
    static MapTilerSample()
    {
        MapTilesDeployer.CopyEmbeddedResourceToFile("TrueMarble");
    }

    public string Name => " 9 Tiles on file system";
    public string Category => "Data Formats";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(CreateLayer());
        return map;
    }

    public static ILayer CreateLayer()
    {
        return new TileLayer(new MapTilerTileSource()) { Name = "True Marble in MapTiler" };
    }
}

public class MapTilerTileSource : ITileSource
{
    public MapTilerTileSource()
    {
        Schema = GetTileSchema();
        Provider = GetTileProvider();
        Name = "MapTiler";
    }

    public ITileSchema Schema { get; }
    public string Name { get; }
    public Attribution Attribution { get; } = new Attribution();
    public ITileProvider Provider { get; }

    public async Task<byte[]> GetTileAsync(TileInfo tileInfo)
    {
        return await Provider.GetTileAsync(tileInfo);
    }

    public static ITileProvider GetTileProvider()
    {
        var trueMarblePath = Path.Combine(MapTilesDeployer.MapTileLocation, "TrueMarble");
        return new FileTileProvider(new FileCache(trueMarblePath, "png"));
    }

    public static ITileSchema GetTileSchema()
    {
        var schema = new GlobalSphericalMercator(YAxis.TMS);
        schema.Resolutions.Clear();
        schema.Resolutions[0] = new Resolution(0, 156543.033900000);
        schema.Resolutions[1] = new Resolution(1, 78271.516950000);
        return schema;
    }
}
