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

            var line = WorldToScreen(viewport, lineString);
            var path = ToSkia(line);
            

            using (var paint = new SKPaint())
            {
                paint.IsStroke = true;
                paint.StrokeWidth = lineWidth;
                paint.Color = lineColor.ToSkia();
                paint.StrokeJoin = SKStrokeJoin.Round;

                canvas.DrawPath(path, paint);
            }
        }

        private static SKPath ToSkia(List<Point> vertices)
        {
            var points = new SKPath();

            for (var i = 0; i < vertices.Count; i++)
            {
                if (i == 0)
                {
                    points.MoveTo((float)vertices[i].X, (float)vertices[i].Y);
                }
                else
                {
                    points.LineTo((float)vertices[i].X, (float)vertices[i].Y);
                }
            }
            return points;
        }

        private static List<Point> WorldToScreen(IViewport viewport, IEnumerable<Point> points)
        {
            var result = new List<Point>();
            foreach (var point in points)
            {
                result.Add(viewport.WorldToScreen(point.X, point.Y));
            }
            return result;
        }
    }
}