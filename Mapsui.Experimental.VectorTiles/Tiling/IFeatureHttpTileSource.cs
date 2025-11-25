using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BruTile;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Tile source that retrieves features directly via HTTP requests instead of raster tiles
/// </summary>
public interface IFeatureHttpTileSource : ITileSource
{
    /// <summary>
    /// Gets a feature for the specified tile using the provided HttpClient
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for the request</param>
    /// <param name="tileInfo">Information about the tile to fetch</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The feature for the tile, or null if not available</returns>
    Task<IFeature> GetFeatureAsync(HttpClient httpClient, TileInfo tileInfo, CancellationToken? cancellationToken = null);
}
