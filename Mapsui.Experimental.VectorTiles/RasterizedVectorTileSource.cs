using BruTile;
using BruTile.Predefined;
using Mapsui.Experimental.VectorTiles.Rendering;
using System.Threading.Tasks;
using VexTile.Common.Enums;
using VexTile.Common.Sources;
using Mapsui.Experimental.VectorTiles.VexTileCopies;

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
    }

    public ITileSchema Schema => _schema;
    public string Name => "VexTile";
    public Attribution Attribution => new("Attributions");

    public async Task<byte[]?> GetTileAsync(BruTile.TileInfo tileInfo)
    {
        var col = tileInfo.Index.Col;

        // Flip Y only when using OSM axis direction
        var matrixHeight = (int)Schema.GetMatrixHeight(tileInfo.Index.Level);
        var row = Schema.YAxis == YAxis.OSM
            ? matrixHeight - tileInfo.Index.Row - 1
            : tileInfo.Index.Row;

        var tileWidth = _schema.GetTileWidth(tileInfo.Index.Level);
        var tileHeight = _schema.GetTileHeight(tileInfo.Index.Level);

        // Fetch the vector tile
        var vectorTile = await _tileSource.GetVectorTileAsync(col, row, tileInfo.Index.Level);
        if (vectorTile == null)
            return null;

        var vexTileInfo = new VexTile.Renderer.Mvt.AliFlux.TileInfo(col, row, tileInfo.Index.Level, tileWidth, tileHeight);

        // Normalize geometry
        foreach (var vectorLayer in vectorTile.Layers)
        {
            foreach (var feature in vectorLayer.Features)
            {
                foreach (var geometry in feature.Geometry)
                {
                    for (int i = 0; i < geometry.Count; i++)
                    {
                        var point = geometry[i];
                        geometry[i] = new VexTile.Renderer.Mvt.AliFlux.Drawing.Point(
                            point.X / feature.Extent * vexTileInfo.ScaledSizeX,
                            point.Y / feature.Extent * vexTileInfo.ScaledSizeY);
                    }
                }
            }
        }

        // Render using our renderer
        using var canvas = new VexTileCopies.SkiaCanvas((int)vexTileInfo.ScaledSizeX, (int)vexTileInfo.ScaledSizeY);
        VexTileRenderer.Render(vectorTile, _style, canvas, vexTileInfo);
        return canvas.ToPngByteArray();
    }
}
