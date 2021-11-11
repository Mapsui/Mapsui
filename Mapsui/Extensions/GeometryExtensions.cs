using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<GeometryFeature> ToFeatures(this IEnumerable<IGeometry> geometries)
        {
            return geometries.Select(g => new GeometryFeature(g)).ToList();
        }
    }
}