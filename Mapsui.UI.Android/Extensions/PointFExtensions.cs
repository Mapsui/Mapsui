using Android.Graphics;

namespace Mapsui.UI.Android.Extensions
{
    static class PointFExtensions
    {
        public static Geometries.Point ToMapsui(this PointF point)
        {
            return new Geometries.Point(point.X, point.Y);
        }
    }
}