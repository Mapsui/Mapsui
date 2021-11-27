using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LineStringRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            LineString lineString, float opacity)
        {
            if (style is LabelStyle labelStyle)
            {
                if (feature.Extent == null)
                    return;

                var center = viewport.WorldToScreen(feature.Extent.Centroid);
                LabelRenderer.Draw(canvas, labelStyle, feature, center.ToPoint(), opacity);
            }
            else
            {
                float lineWidth = 1;
                var lineColor = new Color();

                var vectorStyle = style as VectorStyle;
                var strokeCap = PenStrokeCap.Butt;
                var strokeJoin = StrokeJoin.Miter;
                var strokeMiterLimit = 4f;
                var strokeStyle = PenStyle.Solid;
                float[]? dashArray = null;
                float dashOffset = 0;

                if (vectorStyle is not null && vectorStyle.Line != null)
                {
                    lineWidth = (float)vectorStyle.Line.Width;
                    lineColor = vectorStyle.Line.Color;
                    strokeCap = vectorStyle.Line.PenStrokeCap;
                    strokeJoin = vectorStyle.Line.StrokeJoin;
                    strokeMiterLimit = vectorStyle.Line.StrokeMiterLimit;
                    strokeStyle = vectorStyle.Line.PenStyle;
                    dashArray = vectorStyle.Line.DashArray;
                    dashOffset = vectorStyle.Line.DashOffset;
                }

                using var path = lineString.Vertices.ToSkiaPath(viewport, canvas.LocalClipBounds);
                using var paint = new SKPaint { IsAntialias = true };
                paint.IsStroke = true;
                paint.StrokeWidth = lineWidth;
                paint.Color = lineColor.ToSkia(opacity);
                paint.StrokeCap = strokeCap.ToSkia();
                paint.StrokeJoin = strokeJoin.ToSkia();
                paint.StrokeMiter = strokeMiterLimit;
                paint.PathEffect = strokeStyle != PenStyle.Solid
                    ? strokeStyle.ToSkia(lineWidth, dashArray, dashOffset)
                    : null;
                canvas.DrawPath(path, paint);
            }
        }
    }
}