using System.Diagnostics.CodeAnalysis;
using NetTopologySuite.Geometries;

namespace Mapsui.Extensions
{
    public static class MPointExtensions
    {
        [return: NotNullIfNotNull("point")]
        public static Point? ToPoint(this MPoint? point)
        {
            if (point == null)
                return null;

            return new Point(point.X, point.Y);
        }

        public static Coordinate? ToCoordinate(this MPoint? point)
        {
            if (point == null)
                return null;

            return new Coordinate(point.X, point.Y);
        }
    }
}
