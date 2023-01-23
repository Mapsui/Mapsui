using System;

namespace Mapsui.Layers;

public class RectFeature : BaseFeature, IFeature
{
    public MRect? Rect { get; set; }
    public MRect? Extent => Rect;

    public RectFeature(RectFeature rectFeature) : base(rectFeature)
    {
        if (rectFeature.Rect != null)
        {
            Rect = new MRect(rectFeature.Rect);
        }
    }

    public RectFeature(MRect rect)
    {
        Rect = rect;
    }

    public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
    {
        if (Rect != null)
            foreach (var point in new[] { Rect.Min, Rect.Max })
                visit(point.X, point.Y, (x, y) =>
                {
                    point.X = x;
                    point.Y = y;
                });
    }
}
