namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Tile source that provides features with a dynamic extent
/// </summary>
public interface IFeatureTileSource
{
    /// <summary>
    /// Gets the extent of the tile source
    /// </summary>
    MRect? Extent { get; }
}
