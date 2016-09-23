using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using OpenTK.Graphics.ES11;
using System.Collections.Generic;

namespace Mapsui.Rendering.OpenTK
{
    class PolygonRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            var lineString = ((Polygon)feature.Geometry).ExteriorRing.Vertices;

            float lineWidth = 1;
            var lineColor = Color.Black; // default
            var fillColor = Color.Gray; // default

            var vectorStyle = style as VectorStyle;

            if (vectorStyle != null)
            {
                lineWidth = (float)vectorStyle.Outline.Width;
                lineColor = vectorStyle.Outline.Color;
                fillColor = vectorStyle.Fill.Color;
            }

            float[] points = ToOpenTK(lineString);
            WorldToScreen(viewport, points);

            // Fill
            // Not implemented. It might be hard to draw a concave polygon with holes.             
            
            // Outline
            GL.LineWidth(lineWidth);
            GL.Color4(lineColor.R, lineColor.G, lineColor.B, lineColor.A);
            GL.EnableClientState(All.VertexArray);
            GL.VertexPointer(2, All.Float, 0, points);
            GL.DrawArrays(All.LineLoop, 0, points.Length / 2);
            GL.DisableClientState(All.VertexArray);
        }

        private static float[] ToOpenTK(IList<Point> vertices)
        {
            const int dimensions = 2; // x and y are both in one array
            var points = new float[vertices.Count * dimensions];

            for (var i = 0; i < vertices.Count; i++)
            {
                points[i * 2 + 0] = (float)vertices[i].X;
                points[i * 2 + 1] = (float)vertices[i].Y;
            }

            return points;
        }

        private static void WorldToScreen(IViewport viewport, float[] points)
        {
            for (var i = 0; i < points.Length / 2; i++)
            {
                var point = viewport.WorldToScreen(points[i * 2], points[i * 2 + 1]);
                points[i * 2] = (float)point.X;
                points[i * 2 + 1] = (float)point.Y;
            }
        }
    }
}
