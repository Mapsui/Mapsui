using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class MPointExtensions
    {
        public static Point ToPoint(this MPoint point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
