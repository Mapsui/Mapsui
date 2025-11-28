using Mapsui.Styles;

namespace Mapsui.Experimental.VectorTiles.Tiling;

public class VectorTileStyle(IStyle style) : BaseStyle
{
    // Vector tile rendering has two stages.
    // 1. The VectorTileStyle on the layer used to select the VectorTileRenderer.
    // 2. The VectorTileRenderer uses the Style within the VectorTileStyle
    // to draw with individual features within the vector tile.
    public IStyle Style { get; } = style;
}
