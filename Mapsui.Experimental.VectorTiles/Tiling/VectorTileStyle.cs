using Mapsui.Styles;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Style for vector tiles.
/// </summary>
/// <param name="style">The style to apply to the vector tile features.</param>
public class VectorTileStyle(IStyle style) : BaseStyle
{
    // Vector tile rendering has two stages.
    // 1. The VectorTileStyle on the layer used to select the VectorTileRenderer.
    // 2. The VectorTileRenderer uses the Style within the VectorTileStyle
    // to draw with individual features within the vector tile.
    /// <summary>
    /// The style to apply to the vector tile features.
    /// </summary>
    public IStyle Style { get; } = style;
}
