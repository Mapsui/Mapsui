using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

internal static class PolygonRenderer
{
    /// <summary>
    /// fill paint scale
    /// </summary>
    private const float Scale = 10.0f;

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, VectorStyle vectorStyle, IFeature feature,
        Polygon polygon, float opacity, ISymbolCache? symbolCache = null, IVectorCache? vectorCache = null)
    {


        float lineWidth = Convert.ToSingle(vectorStyle?.Outline?.Width ?? 1);
        SKPaint paint;
        SKPaint paintFill;
        SKPath path;
        if (vectorCache == null)
        {
            paint = CreateSkPaint(vectorStyle?.Outline, opacity);
            paintFill = CreateSkPaint(vectorStyle?.Fill, opacity, viewport.Rotation, symbolCache);
            path = polygon.ToSkiaPath(viewport, canvas.LocalClipBounds, lineWidth);
        }
        else
        {
            paint = vectorCache.GetOrCreatePaint(vectorStyle?.Outline, opacity, CreateSkPaint);
            paintFill = vectorCache.GetOrCreatePaint(vectorStyle?.Fill, opacity, viewport.Rotation, CreateSkPaint);
            path = vectorCache.GetOrCreatePath(viewport, polygon, lineWidth, (geometry, viewport, lineWidth) => geometry.ToSkiaPath(viewport, viewport.ToSkiaRect(), lineWidth));
        }

        if (vectorStyle?.Fill?.FillStyle == FillStyle.Solid)
        {
            canvas.DrawPath(path, paintFill);
        }
        else
        {
            // Do this, because if not, path isn't filled complete
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipPath(path);
                var bounds = path.Bounds;
                // Make sure, that the brush starts with the correct position
                var inflate = ((int)path.Bounds.Width * 0.3f / Scale) * Scale;
                bounds.Inflate(inflate, inflate);
                // Draw rect with bigger size, which is clipped by path
                canvas.DrawRect(bounds, paintFill);
            }
        }

        if (vectorStyle?.Outline != null)
        {
            canvas.DrawPath(path, paint);
        }
    }

    private static SKPaint CreateSkPaint(Brush? brush, float opacity, double rotation, ISymbolCache? symbolCache)
    {
        var fillColor = Color.Gray; // default

        var paintFill = new SKPaint { IsAntialias = true };

        if (brush != null)
        {
            fillColor = brush.Color;
        }

        // Is there a FillStyle?
        if (brush?.FillStyle == FillStyle.Solid)
        {
            paintFill.StrokeWidth = 0;
            paintFill.Style = SKPaintStyle.Fill;
            paintFill.PathEffect = null;
            paintFill.Shader = null;
            paintFill.Color = fillColor.ToSkia(opacity);
        }
        else
        {
            paintFill.StrokeWidth = 1;
            paintFill.Style = SKPaintStyle.Stroke;
            paintFill.Shader = null;
            paintFill.Color = fillColor.ToSkia(opacity);
            using var fillPath = new SKPath();
            var matrix = SKMatrix.CreateScale(Scale, Scale);

            switch (brush?.FillStyle)
            {
                case FillStyle.Cross:
                    fillPath.MoveTo(Scale * 0.8f, Scale * 0.8f);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, Scale * 0.8f);
                    fillPath.LineTo(Scale * 0.8f, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.DiagonalCross:
                    fillPath.MoveTo(Scale, Scale);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, Scale);
                    fillPath.LineTo(Scale, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.BackwardDiagonal:
                    fillPath.MoveTo(0, Scale);
                    fillPath.LineTo(Scale, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.ForwardDiagonal:
                    fillPath.MoveTo(Scale, Scale);
                    fillPath.LineTo(0, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Dotted:
                    paintFill.Style = SKPaintStyle.StrokeAndFill;
                    fillPath.AddCircle(Scale * 0.5f, Scale * 0.5f, Scale * 0.35f);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Horizontal:
                    fillPath.MoveTo(0, Scale * 0.5f);
                    fillPath.LineTo(Scale, Scale * 0.5f);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Vertical:
                    fillPath.MoveTo(Scale * 0.5f, 0);
                    fillPath.LineTo(Scale * 0.5f, Scale);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Bitmap:
                    paintFill.Style = SKPaintStyle.Fill;
                    var image = GetImage(symbolCache, brush.BitmapId);
                    if (image != null)
                        paintFill.Shader = image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                    break;
                case FillStyle.BitmapRotated:
                    paintFill.Style = SKPaintStyle.Fill;
                    image = GetImage(symbolCache, brush.BitmapId);
                    if (image != null)
                        paintFill.Shader = image.ToShader(SKShaderTileMode.Repeat,
                            SKShaderTileMode.Repeat,
                            SKMatrix.CreateRotation((float)(rotation * System.Math.PI / 180.0f),
                                image.Width >> 1, image.Height >> 1));
                    break;
                default:
                    paintFill.PathEffect = null;
                    break;
            }
        }

        return paintFill;
    }

    private static SKPaint CreateSkPaint(Pen? pen, float opacity)
    {
        float lineWidth = 1;
        var lineColor = Color.Black; // default
        var strokeCap = PenStrokeCap.Butt; // default
        var strokeJoin = StrokeJoin.Miter; // default
        var strokeMiterLimit = 4f; // default
        var strokeStyle = PenStyle.Solid; // default
        float[]? dashArray = null; // default
        float dashOffset = 0; // default

        if (pen != null)
        {
            lineWidth = Convert.ToSingle(pen.Width);
            lineColor = pen.Color;
            strokeCap = pen.PenStrokeCap;
            strokeJoin = pen.StrokeJoin;
            strokeMiterLimit = pen.StrokeMiterLimit;
            strokeStyle = pen.PenStyle;
            dashArray = pen.DashArray;
            dashOffset = pen.DashOffset;
        }

        var paintStroke = new SKPaint { IsAntialias = true };
        {
            paintStroke.Style = SKPaintStyle.Stroke;
            paintStroke.StrokeWidth = lineWidth;
            paintStroke.Color = lineColor.ToSkia(opacity);
            paintStroke.StrokeCap = strokeCap.ToSkia();
            paintStroke.StrokeJoin = strokeJoin.ToSkia();
            paintStroke.StrokeMiter = strokeMiterLimit;
            if (strokeStyle != PenStyle.Solid)
                paintStroke.PathEffect = strokeStyle.ToSkia(lineWidth, dashArray, dashOffset);
            else
                paintStroke.PathEffect = null;
        }

        return paintStroke;
    }

    private static SKImage? GetImage(ISymbolCache? symbolCache, int bitmapId)
    {
        if (symbolCache == null)
            return null;
        var bitmapInfo = (BitmapInfo)symbolCache.GetOrCreate(bitmapId);
        if (bitmapInfo == null)
            return null;
        if (bitmapInfo.Type == BitmapType.Bitmap)
            return bitmapInfo.Bitmap;
        if (bitmapInfo.Type == BitmapType.Sprite)
        {
            var sprite = bitmapInfo.Sprite;
            if (sprite == null)
                return null;

            if (sprite.Data == null)
            {
                var bitmapAtlas = (BitmapInfo)symbolCache.GetOrCreate(sprite.Atlas);
                sprite.Data = bitmapAtlas?.Bitmap?.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                    sprite.Y + sprite.Height));
            }
            return (SKImage?)sprite.Data;
        }
        return null;
    }
}
