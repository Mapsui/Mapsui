using BruTile;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Layers;
using Mapsui.Tiling.Extensions;
using NetTopologySuite.IO.VectorTiles;
using System;

namespace Mapsui.Experimental.VectorTiles.Tiling;

public class VectorTileFeature(VectorTile vectorTile, TileInfo tileInfo) : BaseFeature
{
    public VectorTile VectorTile { get; } = vectorTile.Copy();

    public override MRect? Extent { get; } = tileInfo.Extent.ToMRect();

    public override object Clone()
    {
        return new VectorTileFeature(vectorTile, tileInfo);
    }

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
