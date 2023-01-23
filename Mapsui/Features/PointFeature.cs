using System;

namespace Mapsui.Layers;

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

    public MPoint Point { get; }
    public MRect Extent => Point.MRect;

    public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        visit(Point.X, Point.Y, (x, y) =>
        {
            Point.X = x;
            Point.Y = y;
        });
    }
}
