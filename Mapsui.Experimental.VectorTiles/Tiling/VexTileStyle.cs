using Mapsui.Styles;
using VexVectorStyle = VexTile.Renderer.Mvt.AliFlux.VectorStyle;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Style for VexTile features. This style triggers the VexTileStyleRenderer.
/// </summary>
public class VexTileStyle : BaseStyle
{
    /// <summary>
    /// The VexTile vector style containing styling rules.
    /// </summary>
    public VexVectorStyle VexStyle { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VexTileStyle"/> class.
    /// </summary>
    /// <param name="vexStyle">The VexTile vector style containing styling rules.</param>
    public VexTileStyle(VexVectorStyle vexStyle)
    {
        VexStyle = vexStyle;
    }
}
