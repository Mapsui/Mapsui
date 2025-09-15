using BruTile;
using BruTile.Predefined;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Common.Sources;
using VexTile.Data.Sources;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Sources;

namespace Mapsui.Samples.Common.Maps.Tiles;

public sealed class RasterizedVectorTilesSample : ISample, IDisposable
{
    SqliteDataSource _sqliteDataSource;

    static RasterizedVectorTilesSample()
    {
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");
    }

    public RasterizedVectorTilesSample()
    {
        _sqliteDataSource = CreateSqliteDataSource();
    }

    public string Name => "Rasterized Vector Tiles";
    public string Category => "Tiles";

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

        var tileSource = new VectorTileSource(sqliteDataSource);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
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

    private sealed class VectorTileSource : ILocalTileSource
    {
        private readonly VectorTilesSource _tileSource;
        private readonly VectorStyle _style = new(VectorStyleKind.Default);
        public ITileSchema Schema => new GlobalSphericalMercator { YAxis = YAxis.OSM };
        public string Name => "VexTile";
        public Attribution Attribution => new("Attributions");

        public VectorTileSource(ITileDataSource tileDataSource)
        {
            _tileSource = new VectorTilesSource(tileDataSource);
            _style.SetSourceProvider("openmaptiles", _tileSource);
        }

        public Task<byte[]?> GetTileAsync(BruTile.TileInfo tileInfo)
        {
            var canvas = new SkiaCanvas();
            return TileRendererFactory.RenderAsync(_style, canvas, tileInfo.Index.Col, (int)Schema.GetMatrixHeight(tileInfo.Index.Level) - tileInfo.Index.Row - 1, tileInfo.Index.Level, 256, 256);
        }
    }
}
