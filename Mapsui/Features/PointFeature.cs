using System;

namespace Mapsui.Layers;

/// <summary>
/// Feature representing a point on the <cref="Map"/>
/// </summary>
public class PointFeature : BaseFeature, IFeature
{
    public PointFeature(PointFeature pointFeature) : base(pointFeature)
    {
        Point = new MPoint(pointFeature.Point);
    }

    public PointFeature(MPoint point)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));
    }

    public PointFeature(double x, double y)
    {
        Point = new MPoint(x, y);
    }

    public PointFeature((double x, double y) point)
    {
        Point = new MPoint(point.x, point.y);
    }

    /// <summary>
    /// Point of <cref="Map"/>
    /// </summary>
    public MPoint Point { get; }

    /// <summary>
    /// Extent of feature
    /// </summary>
    public MRect Extent => Point.MRect;

    /// <summary>
    /// Order of feature
    /// </summary>
    public int ZOrder { get; set; } = 0;

    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit"></param>
    public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        visit(Point.X, Point.Y, (x, y) =>
        {
            Point.X = x;
            Point.Y = y;
        });
    }
}
