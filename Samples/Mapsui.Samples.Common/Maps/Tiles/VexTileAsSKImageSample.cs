using BruTile.Predefined;
using Mapsui.Experimental.VectorTiles;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Samples.Common.Utilities;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Data.Sources;
using VexTile.Renderer.Mvt.AliFlux;
using TileLayer = Mapsui.Experimental.VectorTiles.Tiling.TileLayer;

namespace Mapsui.Samples.Common.Maps.Tiles;

/// <summary>
/// Sample demonstrating the VexTileSource which fetches vector tile data
/// and renders it using TwoStepVexTileStyleRenderer with caching.
/// </summary>
public sealed class VexTileAsSKImageSample : ISample, IDisposable
{
    private SqliteDataSource? _sqliteDataSource;

    static VexTileAsSKImageSample()
    {
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");
        SQLitePCL.Batteries.Init();
    }

    public VexTileAsSKImageSample()
    {
        _sqliteDataSource = CreateSqliteDataSource();
    }

    public string Name => "VexTileAsSKImage";
    public string Category => "BetaVectorTiles";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap(_sqliteDataSource!));
    }

    public static Map CreateMap(SqliteDataSource sqliteDataSource)
    {
        var map = new Map();
        map.Layers.Add(CreateLayer(sqliteDataSource));
        return map;
    }

    private static TileLayer CreateLayer(SqliteDataSource sqliteDataSource)
    {
        // Create the VexTile vector style (contains styling rules)
        var vectorStyle = new VectorStyle(VectorStyleKind.Default);

        // Create the tile source (fetches vector tile data)
        var tileSource = new VexTileSource(sqliteDataSource, new GlobalSphericalMercator(0, 14));

        return new TileLayer(tileSource)
        {
            Name = "VexTile",
            Style = new VexTileStyle(vectorStyle),
        };
    }

    private static SqliteDataSource CreateSqliteDataSource()
    {
        var path = Path.Combine(MbTilesDeployer.MbTilesLocation, "zurich.mbtiles");
        var connectionString = new SQLiteConnectionString(path, SQLiteOpenFlags.ReadOnly, false);
        return new SqliteDataSource(connectionString);
    }

    public void Dispose()
    {
        _sqliteDataSource?.Dispose();
        _sqliteDataSource = null;
    }
}
