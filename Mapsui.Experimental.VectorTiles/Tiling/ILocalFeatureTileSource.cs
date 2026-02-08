using System.Threading;
using System.Threading.Tasks;
using BruTile;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Tile source that retrieves features directly from local sources instead of raster tiles
/// </summary>
public interface ILocalFeatureTileSource : ITileSource, IFeatureTileSource
{
    /// <summary>
    /// Gets a feature for the specified tile
    /// </summary>
    /// <param name="tileInfo">Information about the tile to fetch</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The feature for the tile, or null if not available</returns>
    Task<IFeature?> GetFeatureAsync(TileInfo tileInfo, CancellationToken cancellationToken = default);
}
