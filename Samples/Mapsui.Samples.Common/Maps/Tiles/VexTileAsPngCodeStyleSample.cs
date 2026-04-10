using Mapsui.Experimental.VectorTiles;
using Mapsui.Experimental.VectorTiles.VexTileCopies;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using SQLite;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Data.Sources;

namespace Mapsui.Samples.Common.Maps.Tiles;

public sealed class VexTileAsPngCodeStyleSample : ISample, IDisposable
{
    SqliteDataSource _sqliteDataSource;

    static VexTileAsPngCodeStyleSample()
    {
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");
        SQLitePCL.Batteries.Init();
    }

    public VexTileAsPngCodeStyleSample()
    {
        _sqliteDataSource = CreateSqliteDataSource();
    }

    public string Name => "VexTileAsPngCodeStyle";
    public string Category => "ExperimentalVectorTiles";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap(_sqliteDataSource));
    }

    public static Map CreateMap(SqliteDataSource sqliteDataSource)
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayer(sqliteDataSource));

        return map;
    }

    private static TileLayer CreateLayer(SqliteDataSource sqliteDataSource)
    {
        var style = CreateCustomStyle();
        var tileSource = new RasterizedVectorTileSource(sqliteDataSource, style: style);
        return new TileLayer(tileSource, dataFetchStrategy: new MinimalDataFetchStrategy())
        {
            Name = "VexTile.TileSource.Mvt",
        };
    }

    private static VectorStyle CreateCustomStyle()
    {
        var style = new VectorStyle(VectorStyleKind.Default);

        // Remove all symbol layers so no text labels are rendered.
        // Symbol layers render place names, road labels, POI icons, etc.
        style.Layers.RemoveAll(l => l.Type == "symbol");

        // Change water fill color to a more vivid blue.
        // All style modifications must be done before the first tile renders
        // because VectorStyle caches the computed paint values per layer+zoom.
        var water = style.Layers.FirstOrDefault(l => l.ID == "water");
        if (water?.Paint != null)
            water.Paint["fill-color"] = "rgba(64, 164, 223, 1)";

        return style;
    }

    private static SqliteDataSource CreateSqliteDataSource()
    {
        var path = Path.Combine(MbTilesDeployer.MbTilesLocation, "zurich.mbtiles");
        SQLiteConnectionString val = new SQLiteConnectionString(path, (SQLiteOpenFlags)1, false);
        return new SqliteDataSource(val);
    }

    void IDisposable.Dispose()
    {
        _sqliteDataSource.Dispose();
    }
}
