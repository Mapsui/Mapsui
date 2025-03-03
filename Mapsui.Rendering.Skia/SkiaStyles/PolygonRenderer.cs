﻿using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia;

internal static class PolygonRenderer
{
    /// <summary>
    /// fill paint scale
    /// </summary>
    private const float _scale = 10.0f;

    public static void Draw(SKCanvas canvas, Viewport viewport, VectorStyle vectorStyle, IFeature feature,
        Polygon polygon, float opacity, VectorCache vectorCache, int position)
    {
        // polygon - relevant for GeometryCollection children
        SKPath ToPath((long featureId, int position, MRect extent, double rotation, float lineWidth) valueTuple)
        {
            var result = polygon.ToSkiaPath(viewport, viewport.ToSkiaRect(), valueTuple.lineWidth);
            return result;
        }

        if (vectorStyle == null)
            return;

        var extent = viewport.ToExtent();
        var rotation = viewport.Rotation;
        float lineWidth = (float)(vectorStyle.Outline?.Width ?? 1);

        using var path = vectorCache.GetOrCreate((feature.Id, position, extent, rotation, lineWidth), ToPath);
        if (vectorStyle.Fill.IsVisible())
        {
            using var fillPaint = vectorCache.GetOrCreate((vectorStyle.Fill, opacity, viewport.Rotation), CreateSkPaint);
            DrawPath(canvas, vectorStyle, path, fillPaint);
        }

        if (vectorStyle.Outline.IsVisible())
        {
            using var paint = vectorCache.GetOrCreate((vectorStyle.Outline, opacity), CreateSkPaint);
            canvas.DrawPath(path, paint);
        }
    }

    internal static void DrawPath(SKCanvas canvas, VectorStyle vectorStyle, CacheTracker<SKPath> path, CacheTracker<SKPaint> paintFill)
    {
        if (vectorStyle?.Fill?.FillStyle == FillStyle.Solid)
        {
            canvas.DrawPath(path, paintFill);
        }
        else
        {
            // Do this, because if not, path isn't filled complete
            using (new SKAutoCanvasRestore(canvas))
            {
                var skPath = path.Instance;
                canvas.ClipPath(skPath);
                var bounds = skPath.Bounds;
                // Make sure, that the brush starts with the correct position
                var inflate = ((int)skPath.Bounds.Width * 0.3f / _scale) * _scale;
                bounds.Inflate(inflate, inflate);
                // Draw rect with bigger size, which is clipped by path
                canvas.DrawRect(bounds, paintFill);
            }
        }
    }

    internal static SKPaint CreateSkPaint((Brush? brush, float opacity, double rotation) valueTuple, IRenderService renderService)
    {
        var skiaRenderService = (RenderService)renderService;

        var brush = valueTuple.brush;
        var opacity = valueTuple.opacity;
        var rotation = valueTuple.rotation;
        var fillColor = Color.Gray; // default

        var paintFill = new SKPaint { IsAntialias = true };

        if (brush?.Color is not null)
        {
            fillColor = brush.Color.Value;
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
            var matrix = SKMatrix.CreateScale(_scale, _scale);

            switch (brush?.FillStyle)
            {
                case FillStyle.Cross:
                    fillPath.MoveTo(_scale * 0.8f, _scale * 0.8f);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, _scale * 0.8f);
                    fillPath.LineTo(_scale * 0.8f, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.DiagonalCross:
                    fillPath.MoveTo(_scale, _scale);
                    fillPath.LineTo(0, 0);
                    fillPath.MoveTo(0, _scale);
                    fillPath.LineTo(_scale, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.BackwardDiagonal:
                    fillPath.MoveTo(0, _scale);
                    fillPath.LineTo(_scale, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.ForwardDiagonal:
                    fillPath.MoveTo(_scale, _scale);
                    fillPath.LineTo(0, 0);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Dotted:
                    paintFill.Style = SKPaintStyle.StrokeAndFill;
                    fillPath.AddCircle(_scale * 0.5f, _scale * 0.5f, _scale * 0.35f);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Horizontal:
                    fillPath.MoveTo(0, _scale * 0.5f);
                    fillPath.LineTo(_scale, _scale * 0.5f);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Vertical:
                    fillPath.MoveTo(_scale * 0.5f, 0);
                    fillPath.LineTo(_scale * 0.5f, _scale);
                    paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                    break;
                case FillStyle.Bitmap:
                    paintFill.Style = SKPaintStyle.Fill;
                    var skImage = GetSKImage(skiaRenderService, brush.Image ?? throw new Exception("Image can not be null when FillStyle is Bitmap"));
                    if (skImage != null)
                        paintFill.Shader = skImage.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                    break;
                case FillStyle.BitmapRotated:
                    paintFill.Style = SKPaintStyle.Fill;
                    var skRotatedImage = GetSKImage(skiaRenderService, brush.Image ?? throw new Exception("Image can not be null when FillStyle is BitmapRotated"));
                    if (skRotatedImage != null)
                        paintFill.Shader = skRotatedImage.ToShader(SKShaderTileMode.Repeat,
                            SKShaderTileMode.Repeat,
                            SKMatrix.CreateRotation((float)(rotation * Math.PI / 180.0f),
                                skRotatedImage.Width >> 1, skRotatedImage.Height >> 1));
                    break;
                default:
                    paintFill.PathEffect = null;
                    break;
            }
        }

        return paintFill;
    }

    internal static SKPaint CreateSkPaint((Pen? pen, float opacity) valueTuple)
    {
        var pen = valueTuple.pen;
        var opacity = valueTuple.opacity;
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
            lineWidth = (float)pen.Width;
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

    private static SKImage? GetSKImage(RenderService renderService, Image image)
    {
        if (image is null)
            return null;
        var drawableImage = renderService.DrawableImageCache.GetOrCreate(image.SourceId,
            () => SymbolStyleRenderer.TryCreateDrawableImage(image, renderService.ImageSourceCache));
        if (drawableImage == null)
            return null;

        if (drawableImage is BitmapDrawableImage bitmapImage)
        {
            if (image.BitmapRegion is null)
                return bitmapImage.Image;

            var imageRegionKey = image.GetSourceIdForBitmapRegion();
            var regionDrawableImage = renderService.DrawableImageCache.GetOrCreate(imageRegionKey, () => CreateBitmapImage(bitmapImage.Image, image.BitmapRegion));
            if (regionDrawableImage == null)
                return null;
            if (regionDrawableImage is BitmapDrawableImage regionBitmapImage)
                return regionBitmapImage.Image;
            throw new Exception("Only bitmaps are is supported for polygon fill.");
        }
        throw new Exception("Only bitmaps are is supported for polygon fill.");
    }

    private static BitmapDrawableImage CreateBitmapImage(SKImage skImage, BitmapRegion bitmapRegion)
    {
        return new BitmapDrawableImage(skImage.Subset(new SKRectI(bitmapRegion.X, bitmapRegion.Y,
            bitmapRegion.X + bitmapRegion.Width, bitmapRegion.Y + bitmapRegion.Height)));
    }
}
