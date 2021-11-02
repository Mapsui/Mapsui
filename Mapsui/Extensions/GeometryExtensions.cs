using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Extensions
{
    public static class GeometryExtensions
    {
        public static IGeometry Copy(this IGeometry original)
        {
            return Geometry.GeomFromWKB(original.AsBinary());
        }

        public static IGeometryFeature ToFeature(this IGeometry geometry)
        {
            return new GeometryFeature { Geometry = geometry };
        }
    }
}