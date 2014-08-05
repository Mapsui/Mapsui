using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Mapsui.Rendering.OpenTK
{
    public class LineStringRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            var lineString = ((LineString)feature.Geometry).Vertices;

            float lineWidth = 1;
            var lineColor = Color.White;

            var vectorStyle = style as VectorStyle;

            if (vectorStyle != null)
            {
                lineWidth = (float)vectorStyle.Line.Width;
                lineColor = vectorStyle.Line.Color;
            }

            float[] points = ToOpenTK(lineString);
            WorldToScreen(viewport, points);

            GL.Color4((byte)lineColor.R, (byte)lineColor.G, (byte)lineColor.B, (byte)lineColor.A);
            GL.EnableClientState(EnableCap.VertexArray);
            GL.LineWidth(lineWidth);
            GL.VertexPointer(2, VertexPointerType.Float, 0, points);
            GL.DrawArrays(PrimitiveType.Lines, 0, points.Length / 2);
            GL.DisableClientState(EnableCap.VertexArray);
            GL.Enable(EnableCap.LineSmooth);
        }

        private static float[] ToOpenTK(IList<Point> vertices)
        {
            const int dimensions = 2; // x and y are both in one array
            int numberOfCoordinates = vertices.Count * 2 - 2; // Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate
            var points = new float[numberOfCoordinates * dimensions];

            for (var i = 0; i < vertices.Count - 1; i++)
            {
                points[i * 4 + 0] = (float)vertices[i].X;
                points[i * 4 + 1] = (float)vertices[i].Y;
                points[i * 4 + 2] = (float)vertices[i + 1].X;
                points[i * 4 + 3] = (float)vertices[i + 1].Y;
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
