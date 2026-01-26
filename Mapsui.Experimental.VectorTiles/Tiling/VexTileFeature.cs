using Mapsui.Layers;
using Mapsui.Tiling.Extensions;
using System;
using BruTileInfo = BruTile.TileInfo;
using VexTileInfo = VexTile.Renderer.Mvt.AliFlux.TileInfo;
using VexVectorTile = VexTile.Renderer.Mvt.AliFlux.VectorTile;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Feature representing a VexTile vector tile with its render parameters.
/// The actual rendering to SKImage happens in the VexTileStyleRenderer.
/// </summary>
public class VexTileFeature : BaseFeature
{
    /// <summary>
    /// The vector tile data.
    /// </summary>
    public VexVectorTile VectorTile { get; }

    /// <summary>
    /// The VexTile tile info (render parameters: col, row, zoom, width, height).
    /// </summary>
    public VexTileInfo VexTileInfo { get; }

    /// <summary>
    /// The BruTile tile info (for extent and positioning).
    /// </summary>
    public BruTileInfo TileInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VexTileFeature"/> class.
    /// </summary>
    /// <param name="vectorTile">The vector tile data.</param>
    /// <param name="vexTileInfo">The VexTile render parameters.</param>
    /// <param name="tileInfo">The BruTile tile info.</param>
    public VexTileFeature(VexVectorTile vectorTile, VexTileInfo vexTileInfo, BruTileInfo tileInfo)
    {
        VectorTile = vectorTile;
        VexTileInfo = vexTileInfo;
        TileInfo = tileInfo;
        Extent = tileInfo.Extent.ToMRect();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VexTileFeature"/> class for cloning.
    /// </summary>
    /// <param name="source">The source feature to clone.</param>
    private VexTileFeature(VexTileFeature source) : base(source)
    {
        VectorTile = source.VectorTile;
        VexTileInfo = source.VexTileInfo;
        TileInfo = source.TileInfo;
        Extent = source.Extent;
    }

    /// <inheritdoc />
    public override MRect? Extent { get; }

    /// <inheritdoc />
    public override object Clone()
    {
        return new VexTileFeature(this);
    }

    /// <inheritdoc />
    public override void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        // VexTileFeature contains vector data, but coordinates are in tile-local space
    }
}
