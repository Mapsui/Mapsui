using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity)
        {
            if (style is LabelStyle labelStyle)
            {
                var worldCenter = geometry.BoundingBox.Centroid;
                var center = viewport.WorldToScreen(worldCenter);
                LabelRenderer.Draw(canvas, labelStyle, feature, (float) center.X, (float) center.Y, opacity);
            }
            else
            {
                var lineString = (LineString)geometry;

                float lineWidth = 1;
                var lineColor = new Color();

                var vectorStyle = style as VectorStyle;
                var strokeCap = PenStrokeCap.Butt;
                var strokeJoin = StrokeJoin.Miter;
                var strokeMiterLimit = 4f;
                var strokeStyle = PenStyle.Solid;
                float[] dashArray = null;

                if (vectorStyle != null)
                {
                    lineWidth = (float) vectorStyle.Line.Width;
                    lineColor = vectorStyle.Line.Color;
                    strokeCap = vectorStyle.Line.PenStrokeCap;
                    strokeJoin = vectorStyle.Line.StrokeJoin;
                    strokeMiterLimit = vectorStyle.Line.StrokeMiterLimit;
                    strokeStyle = vectorStyle.Line.PenStyle;
                    dashArray = vectorStyle.Line.DashArray;
                }

                var path = lineString.Vertices.ToSkiaPath(viewport, canvas.LocalClipBounds);

                using (var paint = new SKPaint { IsAntialias = true })
                {
                    paint.IsStroke = true;
                    paint.StrokeWidth = lineWidth;
                    paint.Color = lineColor.ToSkia(opacity);
                    paint.StrokeCap = strokeCap.ToSkia();
                    paint.StrokeJoin = strokeJoin.ToSkia();
                    paint.StrokeMiter = strokeMiterLimit;
                    if (strokeStyle != PenStyle.Solid)
                        paint.PathEffect = strokeStyle.ToSkia(lineWidth, dashArray);
                    else
                        paint.PathEffect = null;
                    canvas.DrawPath(path, paint);
                }

                if (style is ArrowVectorStyle arrowStyle)
                {
                    var arrowHeadPosition = arrowStyle.GetArrowHeadPosition(lineString.StartPoint, lineString.EndPoint);
                    var arrowHeadScreenPosition = viewport.WorldToScreen(arrowHeadPosition);
                    var arrowBranchesEndPoints = arrowStyle.GetArrowEndPoints(lineString);
                    var deltaFirstEndpoint = new Point(arrowBranchesEndPoints[0].X - arrowHeadPosition.X, arrowBranchesEndPoints[0].Y - arrowHeadPosition.Y);
                    var deltaSecondEndpoint = new Point(arrowBranchesEndPoints[1].X - arrowHeadPosition.X, arrowBranchesEndPoints[1].Y - arrowHeadPosition.Y);

                    DrawArrow(canvas, style, arrowHeadScreenPosition, deltaFirstEndpoint, deltaSecondEndpoint, opacity);
                }
            }
        }

        private static void DrawArrow(SKCanvas canvas, IStyle style, Point destination, Point firstEndpoint, Point secondEndpoint, float opacity)
        {
            var vectorStyle = style is VectorStyle ? (VectorStyle)style : new VectorStyle();
            canvas.Save();
            canvas.Translate((float)destination.X, (float)destination.Y);

            var path = new SKPath();
            path.MoveTo(0, 0);
            path.LineTo((float)firstEndpoint.X, -1 * (float)firstEndpoint.Y);
            path.MoveTo(0, 0);
            path.LineTo((float)secondEndpoint.X, -1 * (float)secondEndpoint.Y);

            var linePaint = CreateLinePaint(vectorStyle.Line, opacity);
            if ((linePaint != null) && linePaint.Color.Alpha != 0)
            {
                canvas.DrawPath(path, linePaint);
            }

            canvas.Restore();
        }

        private static SKPaint CreateLinePaint(Pen line, float opacity)
        {
            if (line == null)
            {
                return null;
            }

            return new SKPaint
            {
                Color = line.Color.ToSkia(opacity),
                StrokeWidth = (float)line.Width,
                StrokeCap = line.PenStrokeCap.ToSkia(),
                PathEffect = line.PenStyle.ToSkia((float)line.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }
    }
}