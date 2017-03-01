using System.Collections.Generic;
using Mapsui.Geometries;
using Point = Mapsui.Geometries.Point;
using XamlMedia = System.Windows.Media;
using XamlPoint = System.Windows.Point;

namespace Mapsui.Rendering.Xaml
{
    static class GeometryExtensions
    {
        public static XamlPoint ToXaml(this Point point)
        {
            return new XamlPoint(point.X, point.Y);
        }

        public static XamlMedia.Geometry ToXaml(this LineString lineString)
        {
            var pathGeometry = new XamlMedia.PathGeometry();
            pathGeometry.Figures.Add(CreatePathFigure(lineString));
            return pathGeometry;
        }

        public static XamlMedia.Geometry ToXaml(this MultiLineString multiLineString)
        {
            var group = new XamlMedia.GeometryGroup();
            foreach (var geometry in multiLineString)
            {
                var lineString = (LineString) geometry;
                group.Children.Add(ToXaml(lineString));
            }
            return group;
        }

        public static XamlMedia.PathGeometry ToXaml(this IEnumerable<LinearRing> linearRings)
        {
            var pathGeometry = new XamlMedia.PathGeometry();
            foreach (var linearRing in linearRings)
                pathGeometry.Figures.Add(CreatePathFigure(linearRing));
            return pathGeometry;
        }

        public static XamlMedia.GeometryGroup ToXaml(this Polygon polygon)
        {
            var group = new XamlMedia.GeometryGroup();
            group.FillRule = XamlMedia.FillRule.EvenOdd;
            group.Children.Add(polygon.ExteriorRing.ToXaml());
            group.Children.Add(polygon.InteriorRings.ToXaml());
            return group;
        }

        public static XamlMedia.GeometryGroup ToXaml(this MultiPolygon geometry)
        {
            var group = new XamlMedia.GeometryGroup();
            foreach (Polygon polygon in geometry.Polygons)
                group.Children.Add(polygon.ToXaml());
            return group;
        }

        private static XamlMedia.PathFigure CreatePathFigure(LineString lineString)
        {
            var pathFigure = new XamlMedia.PathFigure();
            pathFigure.IsClosed = lineString.IsClosed; //changed by report of Akrog (item 9194)

            bool first = true;

            foreach (var point in lineString.Vertices)
            {
                if (first)
                {
                    pathFigure.StartPoint = point.ToXaml();
                    first = false;
                }
                else
                {
                    pathFigure.Segments.Add(new XamlMedia.LineSegment { Point = point.ToXaml() });
                }
            }
            return pathFigure;
        }
    }
}