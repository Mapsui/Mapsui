using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class GeometryExtensions
    {
        public static IGeometry Copy(this IGeometry original)
        {
            return Geometry.GeomFromWKB(original.AsBinary());
        }
    }
}