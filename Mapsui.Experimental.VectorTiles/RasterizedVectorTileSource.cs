using BruTile;
using BruTile.Predefined;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Common.Sources;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Sources;

namespace Mapsui.Experimental.VectorTiles;

public sealed class RasterizedVectorTileSource : ILocalTileSource
{
    private readonly VectorTilesSource _tileSource;
    private readonly VectorStyle _style = new(VectorStyleKind.Default);
    private readonly ITileSchema _schema;

    public RasterizedVectorTileSource(ITileDataSource tileDataSource, ITileSchema? schema = null)
    {
        _schema = schema ?? new GlobalSphericalMercator { YAxis = YAxis.OSM };
        _tileSource = new VectorTilesSource(tileDataSource);
        _style.SetSourceProvider("openmaptiles", _tileSource);
    }

    public ITileSchema Schema => _schema;
    public string Name => "VexTile";
    public Attribution Attribution => new("Attributions");

    public async Task<byte[]?> GetTileAsync(BruTile.TileInfo tileInfo)
    {
        var canvas = new SkiaCanvas();
        var col = tileInfo.Index.Col;

        // Flip Y only when using OSM axis direction
        var matrixHeight = (int)Schema.GetMatrixHeight(tileInfo.Index.Level);
        var row = Schema.YAxis == YAxis.OSM
            ? matrixHeight - tileInfo.Index.Row - 1
            : tileInfo.Index.Row;

        var tileWidth = _schema.GetTileWidth(tileInfo.Index.Level);
        var tileHeight = _schema.GetTileHeight(tileInfo.Index.Level);
        await TileRendererFactory.RenderAsync(_style, canvas, col, row, tileInfo.Index.Level,
            tileWidth, tileHeight);
        return canvas.ToPngByteArray();
    }
}
