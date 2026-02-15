using BruTile;
using BruTile.Predefined;
using Mapsui.Experimental.VectorTiles.Tiling;
using System.Threading;
using System.Threading.Tasks;
using VexTile.Common.Sources;
using VexTile.Renderer.Mvt.AliFlux.Sources;
using VexTileInfo = VexTile.Renderer.Mvt.AliFlux.TileInfo;

namespace Mapsui.Experimental.VectorTiles;

/// <summary>
/// VexTile source that fetches vector tile data and returns VexTileFeature.
/// The actual rendering to SKImage happens in TwoStepVexTileStyleRenderer.
/// </summary>
public sealed class VexTileSource : ILocalFeatureTileSource
{
    private readonly VectorTilesSource _tileSource;
    private readonly ITileSchema _schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="VexTileSource"/> class.
    /// </summary>
    /// <param name="tileDataSource">The tile data source.</param>
    /// <param name="schema">The tile schema. If null, uses GlobalSphericalMercator with OSM Y-axis.</param>
    public VexTileSource(ITileDataSource tileDataSource, ITileSchema? schema = null)
    {
        _schema = schema ?? new GlobalSphericalMercator { YAxis = YAxis.OSM };
        _tileSource = new VectorTilesSource(tileDataSource);
    }

    /// <inheritdoc />
    public ITileSchema Schema => _schema;

    /// <inheritdoc />
    public string Name => "VexTile";

    /// <inheritdoc />
    public Attribution Attribution => new("Attributions");

    /// <inheritdoc />
    public async Task<IFeature?> GetFeatureAsync(TileInfo tileInfo, CancellationToken cancellationToken = default)
    {
        var col = tileInfo.Index.Col;

        // Flip Y only when using OSM axis direction
        var matrixHeight = (int)Schema.GetMatrixHeight(tileInfo.Index.Level);
        var row = Schema.YAxis == YAxis.OSM
            ? matrixHeight - tileInfo.Index.Row - 1
            : tileInfo.Index.Row;

        // Fetch the vector tile data (async)
        var vectorTile = await _tileSource.GetVectorTileAsync(col, row, tileInfo.Index.Level);
        if (vectorTile == null)
            return null;

        // Create VexTileInfo for rendering parameters
        var tileWidth = _schema.GetTileWidth(tileInfo.Index.Level);
        var tileHeight = _schema.GetTileHeight(tileInfo.Index.Level);
        var vexTileInfo = new VexTileInfo(col, row, tileInfo.Index.Level, tileWidth, tileHeight);

        // Normalize geometry coordinates at fetch time (background thread)
        // so that the render thread does not need to mutate the geometry.
        NormalizeGeometry(vectorTile, vexTileInfo.ScaledSizeX, vexTileInfo.ScaledSizeY);

        // Return feature with data - rendering happens in TwoStepVexTileStyleRenderer
        return new VexTileFeature(vectorTile, vexTileInfo, tileInfo);
    }

    /// <summary>
    /// Normalizes geometry coordinates from tile-internal space (0..extent)
    /// to pixel space (0..sizeX/Y). This mutates the VectorTile in-place
    /// and must only be called once per tile, at fetch time.
    /// </summary>
    private static void NormalizeGeometry(
        VexTile.Renderer.Mvt.AliFlux.VectorTile vectorTile, double sizeX, double sizeY)
    {
        foreach (var vectorLayer in vectorTile.Layers)
        {
            foreach (var feature in vectorLayer.Features)
            {
                foreach (var geometry in feature.Geometry)
                {
                    for (int i = 0; i < geometry.Count; i++)
                    {
                        var point = geometry[i];
                        geometry[i] = new(point.X / feature.Extent * sizeX, point.Y / feature.Extent * sizeY);
                    }
                }
            }
        }
    }
}
