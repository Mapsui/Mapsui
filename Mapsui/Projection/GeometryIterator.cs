using System;
using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public static class GeometryIterator
    {
        public static IEnumerable<Point> AllVertices(this IGeometry geometry)
        {
            if (geometry == null) return Array.Empty<Point>();

            if (geometry is Point point) return new[] { point };
            if (geometry is LineString lineString) return AllVertices(lineString);
            if (geometry is Polygon polygon) return AllVertices(polygon);
            if (geometry is IEnumerable<Geometry> geometries) return AllVertices(geometries);
            
            var format = $"unsupported geometry: {geometry.GetType().Name}";
            throw new NotSupportedException(format);
        }

        private static IEnumerable<Point> AllVertices(LineString lineString)
        {
            if (lineString == null) throw new ArgumentNullException(nameof(lineString));

            return lineString.Vertices;
        }

        private static IEnumerable<Point> AllVertices(Polygon polygon)
        {
            if (polygon == null) throw new ArgumentNullException(nameof(polygon));

            foreach (var point in polygon.ExteriorRing.Vertices)
                yield return point;
            foreach (var ring in polygon.InteriorRings)
                foreach (var point in ring.Vertices)
                    yield return point;
        }

        private static IEnumerable<Point> AllVertices(IEnumerable<Geometry> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            foreach (var geometry in collection)
                foreach (var point in AllVertices(geometry))
                    yield return point;
        }
    }
}
