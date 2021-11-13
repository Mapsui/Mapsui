using System.Diagnostics.CodeAnalysis;
using Mapsui.Geometries;

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
    }
}
