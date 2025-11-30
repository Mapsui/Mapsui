using BruTile;
using Mapsui.Projections;
using NetTopologySuite.IO.VectorTiles;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Tile source that wraps an HttpTileSource and converts raster tiles to features
/// </summary>
/// <remarks>
/// Creates a new FeatureHttpTileSource that wraps an existing HttpTileSource
/// </remarks>
/// <param name="httpTileSource">The underlying HTTP tile source to wrap</param>
public sealed class FeatureHttpTileSource(IHttpTileSource httpTileSource) : IFeatureHttpTileSource
{
    private readonly IHttpTileSource _httpTileSource = httpTileSource;

    /// <inheritdoc />
    public ITileSchema Schema => _httpTileSource.Schema;

    /// <inheritdoc />
    public string Name => _httpTileSource.Name;

    /// <inheritdoc />
    public Attribution Attribution => _httpTileSource.Attribution;

    /// <inheritdoc />
    public async Task<IFeature> GetFeatureAsync(HttpClient httpClient, TileInfo tileInfo, CancellationToken? cancellationToken = null)
    {
        var tileData = await _httpTileSource.GetTileAsync(httpClient, tileInfo, cancellationToken).ConfigureAwait(false);

        if (tileData is null)
            return new VectorTileFeature(new VectorTile(), tileInfo); // Should we return null instead?

        var vectorTile = MvtDecoder.DecodeTile(tileData, tileInfo.Index.Col, tileInfo.Index.Row, tileInfo.Index.Level);
        var vectorTileFeature = new VectorTileFeature(vectorTile, tileInfo);
        // Note, we project to EPSG:3857 here but eventually we need to project to the map's CRS
        ProjectionDefaults.Projection.Project("EPSG:4326", "EPSG:3857", vectorTileFeature);
        return vectorTileFeature;
    }
}
