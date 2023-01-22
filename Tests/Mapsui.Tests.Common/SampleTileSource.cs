using BruTile;
using BruTile.Predefined;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common;

internal class SampleTileSource : ITileSource
{
    public SampleTileSource()
    {
        Schema = GetTileSchema();
        Provider = new SampleTileProvider();
    }

    public ITileSchema Schema { get; }
    public string Name { get; } = "TileSource";
    public Attribution Attribution { get; } = new Attribution();
    public ITileProvider Provider { get; }

    public async Task<byte[]> GetTileAsync(TileInfo tileInfo)
    {
        return await Provider.GetTileAsync(tileInfo);
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
