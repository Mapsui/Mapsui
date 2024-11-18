using System;

namespace Mapsui.Layers;

/// <summary>
/// Feature representing a bitmap on the map.
/// </summary>
public class RasterFeature : BaseFeature, IFeature
{
    public RasterFeature(RasterFeature rasterFeature) : base(rasterFeature)
    {
        Raster = rasterFeature.Raster == null ? null : new MRaster(rasterFeature.Raster);
    }

    public RasterFeature(MRaster? raster)
    {
        Raster = raster;
    }

    /// <summary>
    /// Raster containing rect and bitmap of raster
    /// </summary>
    public MRaster? Raster { get; }

    /// <summary>
    /// Extent of feature
    /// </summary>
    public override MRect? Extent => Raster;

    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit"></param>
    public override void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        if (Raster != null)
            foreach (var point in new[] { Raster.Min, Raster.Max })
                visit(point.X, point.Y, (x, y) =>
                {
                    point.X = x;
                    point.Y = y;
                });
    }

    public override object Clone()
    {
        return new RasterFeature(this);
    }
}
