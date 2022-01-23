using System;

namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRect Rect { get; set; } = default!;
        public MRect Extent => Rect;

        public RectFeature(RectFeature rectFeature) : base(rectFeature)
        {
            Rect = new MRect(rectFeature.Rect);
        }

        public RectFeature(MRect rect)
        {
            Rect = rect;
        }

        public void CoordinateVisitor(Action<double, double, CoordinateSetter> visit)
        {
            foreach (var point in new[] { Rect.Min, Rect.Max })
                visit(point.X, point.Y, (x, y) => {
                    point.X = x;
                    point.Y = x;
                });
        }
    }
}
