using System;
using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public static class GeometryIterator
    {
        public static IEnumerable<Point> AllVertices(this IGeometry geometry)
        {
            if (geometry == null) return new Point[0];

            var point = geometry as Point;
            if (point != null) return new[] { point };
            var lineString = geometry as LineString;
            if (lineString != null) return AllVertices(lineString);
            var polygon = geometry as Polygon; 
            if (polygon != null) return AllVertices(polygon);
            var geometries = geometry as IEnumerable<Geometry>;
            if (geometries != null) return AllVertices(geometries);
            
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
