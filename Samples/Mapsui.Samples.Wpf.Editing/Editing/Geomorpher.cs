using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using Mapsui.Projection;

namespace Mapsui.Samples.Wpf.Editing.Editing
{
    public static class Geomorpher
    {
        public static void Rotate(IGeometry geometry, double degrees, Point center)
        {
            foreach (var vertex in geometry.AllVertices())
            {
                Rotate(vertex, degrees, center);
            }
        }

        private static void Rotate(Point vertex, double degrees, Point center)
        {
            // translate this point back to the center
            var newX = vertex.X - center.X;
            var newY = vertex.Y - center.Y;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            vertex.X = p.X + center.X;
            vertex.Y = p.Y + center.Y;
        }

        public static void Scale(IGeometry geometry, double scale, Point center)
        {
            foreach (var vertex in geometry.AllVertices())
            {
                Scale(vertex, scale, center);
            }
        }

        private static void Scale(Point vertex, double scale, Point center)
        {
            vertex.X = center.X + (vertex.X - center.X) * scale;
            vertex.Y = center.Y + (vertex.Y - center.Y) * scale;
        }
    }
}
