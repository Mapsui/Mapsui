using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

namespace Mapsui.Samples.Wpf.Editing
{
    public static class GeometryExtensions
    {
        /// <summary>
        /// For editing features it is simpler if we can treat al
        /// geometries as a list of lists of points.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static IList<IList<Point>> GetVertexLists(this IGeometry geometry)
        {
            if (geometry is Point point)
            {
                return new List<IList<Point>> { new List<Point>{point} };
            }
            if (geometry is LineString lineString)
            {
                return new List<IList<Point>> {new List<Point>(lineString.Vertices)};
            }
            if (geometry is Polygon polygon)
            {
                var lists = new List<IList<Point>>
                {
                    polygon.ExteriorRing.Vertices
                };
                lists.AddRange(polygon.InteriorRings.Select(i => i.Vertices));
                return lists;
            }
            throw new NotImplementedException();
        }

        public static IList<Point> MainVertices(this IGeometry geometry)
        {
            if (geometry is LineString lineString)
            {
                return lineString.Vertices;
            }
            if (geometry is Polygon polygon)
            {
                return polygon.ExteriorRing.Vertices;
            }
            if (geometry is Point point)
            {
                return new List<Point>{point};
            }
            throw new NotImplementedException();
        }
    }
}