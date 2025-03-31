using System;

namespace Mapsui.Layers;

/// <summary>
/// Feature representing a point on the map
/// </summary>
public class PointFeature : BaseFeature, IFeature
{
    public PointFeature(PointFeature pointFeature) : base(pointFeature)
    {
        Point = new MPoint(pointFeature.Point);
        Extent = Point.GetExtent();
    }

    public PointFeature(MPoint point)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));
        Extent = Point.GetExtent();
    }

    public PointFeature(double x, double y) : this(new MPoint(x, y))
    { }

    public PointFeature((double x, double y) point) : this(new MPoint(point.x, point.y))
    { }

    /// <summary>
    /// The location of the feature.
    /// </summary>
    public MPoint Point { get; }

    /// <summary>
    /// Extent of feature
    /// </summary>
    public override MRect Extent { get; }

    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit"></param>
    public override void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        visit(Point.X, Point.Y, (x, y) =>
        {
            Point.X = x;
            Point.Y = y;
        });
    }

    public override object Clone() => new PointFeature(this);
}
