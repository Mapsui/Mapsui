using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class PointExtensions
    {
        public static MPoint ToMPoint(this Point point)
        {
            return new MPoint(point.X, point.Y);
        }
    }
}
