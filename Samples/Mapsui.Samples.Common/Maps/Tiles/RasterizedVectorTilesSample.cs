using Mapsui.Experimental.VectorTiles;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using VexTile.Data.Sources;

namespace Mapsui.Samples.Common.Maps.Tiles;

public sealed class RasterizedVectorTilesSample : ISample, IDisposable
{
    SqliteDataSource _sqliteDataSource;

    static RasterizedVectorTilesSample()
    {
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");
        SQLitePCL.Batteries.Init();
    }

    public RasterizedVectorTilesSample()
    {
        _sqliteDataSource = CreateSqliteDataSource();
    }

    public string Name => "RasterizedVectorTiles";
    public string Category => "VectorTiles";

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

        var tileSource = new RasterizedVectorTileSource(sqliteDataSource);
        return new TileLayer(tileSource, dataFetchStrategy: new MinimalDataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "VexTile.TileSource.Mvt",
        };
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
