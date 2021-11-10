using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;

namespace Mapsui.Extensions
{
    public static class GeometryExtensions
    {
        public static IGeometry Copy(this IGeometry original)
        {
            return Geometry.GeomFromWKB(original.AsBinary());
        }

        public static IFeature ToFeature(this IGeometry geometry)
        {
            return new GeometryFeature(geometry);
        }
    }
}