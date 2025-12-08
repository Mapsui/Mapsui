using BruTile;
using Mapsui.Experimental.VectorTiles.Extensions;
using Mapsui.Layers;
using Mapsui.Tiling.Extensions;
using NetTopologySuite.IO.VectorTiles;
using System;

namespace Mapsui.Experimental.VectorTiles.Tiling;

public class VectorTileFeature : BaseFeature
{
    public VectorTile VectorTile { get; }
    public TileInfo TileInfo { get; }

    public VectorTileFeature(VectorTile vectorTile, TileInfo tileInfo)
    {
        VectorTile = vectorTile.Copy();
        TileInfo = tileInfo;
        Extent = tileInfo.Extent.ToMRect();
    }

    public VectorTileFeature(VectorTileFeature source) : base(source)
    {
        VectorTile = source.VectorTile.Copy();
        TileInfo = source.TileInfo;
        Extent = source.Extent;
    }

    public override MRect? Extent { get; }

    public override object Clone()
    {
        return new VectorTileFeature(this);
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
