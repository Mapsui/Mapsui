using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

namespace Mapsui.Projection
{
    public static class GeometryIterator
    {
        public static IEnumerable<Point> AllVertices(this IGeometry geometry)
        {
            if (geometry == null)
                return new Point[0];
            if (geometry is Point)
                return new[] { (Point)geometry };
            if (geometry is LineString)
                return AllVertices((LineString)geometry);
            if (geometry is Polygon)
                return AllVertices((Polygon)geometry);
            if (geometry is IEnumerable<Geometry>)
                return AllVertices((IEnumerable<Geometry>)geometry);
            var format = String.Format("unsupported geometry: {0}", geometry.GetType().Name);
            throw new NotSupportedException(format);
        }

        private static IEnumerable<Point> AllVertices(LineString lineString)
        {
            if (lineString == null)
                throw new ArgumentNullException("lineString");
            return lineString.Vertices;
        }

        private static IEnumerable<Point> AllVertices(Polygon polygon)
        {
            if (polygon == null)
                throw new ArgumentNullException("polygon");

            foreach (var point in polygon.ExteriorRing.Vertices)
                yield return point;
            foreach (var ring in polygon.InteriorRings)
                foreach (var point in ring.Vertices)
                    yield return point;
        }

        private static IEnumerable<Point> AllVertices(IEnumerable<Geometry> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            foreach (var geometry in collection)
                foreach (var point in AllVertices(geometry))
                    yield return point;
        }
    }
}
