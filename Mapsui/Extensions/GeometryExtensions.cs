using Mapsui.Geometries;
using Mapsui.GeometryLayer;

namespace Mapsui.Extensions
{
    public static class GeometryExtensions
    {
        public static IGeometry Copy(this IGeometry original)
        {
            return Geometry.GeomFromWKB(original.AsBinary());
        }

        public static GeometryFeature ToFeature(this IGeometry geometry)
        {
            return new GeometryFeature(geometry);
        }
    }
}