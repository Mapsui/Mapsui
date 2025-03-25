using BruTile;
using BruTile.Predefined;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Tiling;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Sources;

namespace Mapsui.Samples.Common.Maps.Tiles;

public class RasterizedVectorTilesSample : ISample
{
    static RasterizedVectorTilesSample()
    {
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");
    }
    public string Name => "Rasterized Vector Tiles";
    public string Category => "1";
    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayer());

        return map;
    }

    private static TileLayer CreateLayer()
    {
        var path = Path.Combine(MbTilesDeployer.MbTilesLocation, "zurich.mbtiles");
        SQLiteConnectionString val = new SQLiteConnectionString(path, (SQLiteOpenFlags)1, false);
#pragma warning disable IDISP001 // Dispose created
        var connection = new SQLiteConnection(val);
#pragma warning restore IDISP001 // Dispose created

        var tileSource = new VectorTileSourceWrapper(connection);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "VexTile.TileSource.Mvt",
        };
    }

    private sealed class VectorTileSourceWrapper : ILocalTileSource
    {
        private readonly VectorTilesSource _tileSource;
        private readonly VectorStyle _style = new(VectorStyleKind.Default);
        public ITileSchema Schema => new GlobalSphericalMercator { YAxis = YAxis.OSM };
        public string Name => "VexTile";
        public Attribution Attribution => new Attribution("Attributions");

        public VectorTileSourceWrapper(SQLiteConnection sqliteConnection)
        {
            _tileSource = new VectorTilesSource(sqliteConnection);
            _style.SetSourceProvider("openmaptiles", _tileSource);
        }

        public Task<byte[]?> GetTileAsync(TileInfo tileInfo)
        {
            var canvas = new SkiaCanvas();
            return TileRendererFactory.RenderAsync(_style, canvas, tileInfo.Index.Col, tileInfo.Index.Row, tileInfo.Index.Level);
        }
    }
}
