using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IGeometry geometry)
        {
            var lineString = ((LineString) geometry).Vertices;

            float lineWidth = 1;
            var lineColor = new Color();

            var vectorStyle = style as VectorStyle;

            if (vectorStyle != null)
            {
                lineWidth = (float) vectorStyle.Line.Width;
                lineColor = vectorStyle.Line.Color;
            }

            var points = ToSkia(lineString);
            WorldToScreen(viewport, points);

            using (var paint = new SKPaint())
            {
                paint.IsStroke = true;
                paint.StrokeWidth = lineWidth;
                paint.Color = lineColor.ToSkia();
                paint.StrokeJoin = SKStrokeJoin.Round;

                // todo: figure out how to draw all segments at once to get round stroke joints
                for (var i = 2; i < points.Length; i = i + 2)
                    canvas.DrawLine(points[i - 2], points[i - 1], points[i], points[i + 1], paint);
            }
        }

        private static float[] ToSkia(IList<Point> vertices)
        {
            const int dimensions = 2; // x and y are both in one array
            var numberOfCoordinates = vertices.Count*2 - 2;
                // Times two because of duplicate begin en end. Minus two because the very begin and end need no duplicate
            var points = new float[numberOfCoordinates*dimensions];

            for (var i = 0; i < vertices.Count - 1; i++)
            {
                points[i*4 + 0] = (float) vertices[i].X;
                points[i*4 + 1] = (float) vertices[i].Y;
                points[i*4 + 2] = (float) vertices[i + 1].X;
                points[i*4 + 3] = (float) vertices[i + 1].Y;
            }
            return points;
        }

        private static void WorldToScreen(IViewport viewport, float[] points)
        {
            for (var i = 0; i < points.Length/2; i++)
            {
                var point = viewport.WorldToScreen(points[i*2], points[i*2 + 1]);
                points[i*2] = (float) point.X;
                points[i*2 + 1] = (float) point.Y;
            }
        }
    }
}