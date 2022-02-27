using System.Windows;

namespace Mapsui.UI.Eto.Extensions
{
    using global::Eto.Drawing;
    public static class Extensions
    {
        public static MPoint ToMapsui(this PointF point)
        {
            return new MPoint(point.X, point.Y);
        }
        public static PointF ToEto(this MPoint point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }
    }
}