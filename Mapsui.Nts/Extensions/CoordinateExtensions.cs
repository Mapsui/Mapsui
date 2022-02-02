using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions
{
    public static class CoordinateExtensions
    {
        public static MPoint ToMPoint(this Coordinate point)
        {
            return new MPoint(point.X, point.Y);
        }
        
        public static Point ToPoint(this Coordinate point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
