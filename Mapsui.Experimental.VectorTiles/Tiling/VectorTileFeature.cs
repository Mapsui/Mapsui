using BruTile;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Layers;
using Mapsui.Tiling.Extensions;
using NetTopologySuite.IO.VectorTiles;
using System;

namespace Mapsui.Experimental.VectorTiles.Tiling;

/// <summary>
/// Feature representing a vector tile.
/// </summary>
public class VectorTileFeature : BaseFeature
{
    /// <summary>
    /// The vector tile data.
    /// </summary>
    public VectorTile VectorTile { get; }

    /// <summary>
    /// The tile info.
    /// </summary>
    public TileInfo TileInfo { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorTileFeature"/> class.
    /// </summary>
    /// <param name="vectorTile">The vector tile.</param>
    /// <param name="tileInfo">The tile info.</param>
    public VectorTileFeature(VectorTile vectorTile, TileInfo tileInfo)
    {
        VectorTile = vectorTile.Copy();
        TileInfo = tileInfo;
        Extent = tileInfo.Extent.ToMRect();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorTileFeature"/> class.
    /// </summary>
    /// <param name="source">The source feature to clone.</param>
    public VectorTileFeature(VectorTileFeature source) : base(source)
    {
        VectorTile = source.VectorTile.Copy();
        TileInfo = source.TileInfo;
        Extent = source.Extent;
    }

    /// <inheritdoc />
    public override MRect? Extent { get; }

    /// <inheritdoc />
    public override object Clone()
    {
        return new VectorTileFeature(this);
    }

    /// <inheritdoc />
    public override void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        foreach (var layer in VectorTile.Layers)
        {
            foreach (var feature in layer.Features)
            {
                var geometry = feature.Geometry;
                if (geometry == null)
                    continue;
                foreach (var coordinate in geometry.Coordinates)
                {
                    visit(coordinate.X, coordinate.Y, (x, y) =>
                    {
                        coordinate.X = x;
                        coordinate.Y = y;
                    });
                }
            }
        }
    }
}
