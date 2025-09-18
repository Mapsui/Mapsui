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

    public RasterizedVectorTileSource(ITileDataSource tileDataSource)
    {
        _tileSource = new VectorTilesSource(tileDataSource);
        _style.SetSourceProvider("openmaptiles", _tileSource);
    }

    public ITileSchema Schema => new GlobalSphericalMercator { YAxis = YAxis.OSM };
    public string Name => "VexTile";
    public Attribution Attribution => new("Attributions");

    public Task<byte[]?> GetTileAsync(BruTile.TileInfo tileInfo)
    {
        var canvas = new SkiaCanvas();
        var col = tileInfo.Index.Col;
        // We have to correct for the Y axis direction here. Eventually we want VexTile to use the BruTile schema so it would adopt the Y axis direction automatically.d
        var row = (int)Schema.GetMatrixHeight(tileInfo.Index.Level) - tileInfo.Index.Row - 1;
        return TileRendererFactory.RenderAsync(_style, canvas, col, row, tileInfo.Index.Level, 256, 256);
    }
}
