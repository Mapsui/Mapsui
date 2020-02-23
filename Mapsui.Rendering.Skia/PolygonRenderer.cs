﻿using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity, SymbolCache symbolCache = null)
        {
            if (style is LabelStyle labelStyle)
            {
                var worldCenter = geometry.BoundingBox.Centroid;
                var center = viewport.WorldToScreen(worldCenter);
                LabelRenderer.Draw(canvas, labelStyle, feature, center, opacity);
            }
            else if (style is StyleCollection styleCollection)
            {
                foreach (var s in styleCollection)
                {
                    Draw(canvas, viewport, s, feature, geometry, opacity, symbolCache);
                }
            }
            else if (style is VectorStyle vectorStyle)
            {
                var polygon = (Polygon)geometry;

                float lineWidth = 1;
                var lineColor = Color.Black; // default
                var fillColor = Color.Gray; // default
                var strokeCap = PenStrokeCap.Butt; // default
                var strokeJoin = StrokeJoin.Miter; // default
                var strokeMiterLimit = 4f; // default
                var strokeStyle = PenStyle.Solid; // default
                float[] dashArray = null; // default
      
                if (vectorStyle.Outline != null)
                {
                    lineWidth = (float)vectorStyle.Outline.Width;
                    lineColor = vectorStyle.Outline.Color;
                    strokeCap = vectorStyle.Outline.PenStrokeCap;
                    strokeJoin = vectorStyle.Outline.StrokeJoin;
                    strokeMiterLimit = vectorStyle.Outline.StrokeMiterLimit;
                    strokeStyle = vectorStyle.Outline.PenStyle;
                    dashArray = vectorStyle.Outline.DashArray;
                }

                if (vectorStyle.Fill != null)
                {
                    fillColor = vectorStyle.Fill?.Color;
                }

                using (var path = polygon.ToSkiaPath(viewport, canvas.LocalClipBounds, lineWidth))
                using (var paintFill = new SKPaint { IsAntialias = true })
                {
                    // Is there a FillStyle?
                    if (vectorStyle.Fill?.FillStyle == FillStyle.Solid)
                    {
                        paintFill.StrokeWidth = 0;
                        paintFill.Style = SKPaintStyle.Fill;
                        paintFill.PathEffect = null;
                        paintFill.Shader = null;
                        paintFill.Color = fillColor.ToSkia(opacity);
                        canvas.DrawPath(path, paintFill);
                    }
                    else
                    {
                        paintFill.StrokeWidth = 1;
                        paintFill.Style = SKPaintStyle.Stroke;
                        paintFill.Shader = null;
                        paintFill.Color = fillColor.ToSkia(opacity);
                        var scale = 10.0f;
                        var fillPath = new SKPath();
                        var matrix = SKMatrix.MakeScale(scale, scale);

                        switch (vectorStyle.Fill?.FillStyle)
                        {
                            case FillStyle.Cross:
                                fillPath.MoveTo(scale * 0.8f, scale * 0.8f);
                                fillPath.LineTo(0, 0);
                                fillPath.MoveTo(0, scale * 0.8f);
                                fillPath.LineTo(scale * 0.8f, 0);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.DiagonalCross:
                                fillPath.MoveTo(scale, scale);
                                fillPath.LineTo(0, 0);
                                fillPath.MoveTo(0, scale);
                                fillPath.LineTo(scale, 0);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.BackwardDiagonal:
                                fillPath.MoveTo(0, scale);
                                fillPath.LineTo(scale, 0);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.ForwardDiagonal:
                                fillPath.MoveTo(scale, scale);
                                fillPath.LineTo(0, 0);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Dotted:
                                paintFill.Style = SKPaintStyle.StrokeAndFill;
                                fillPath.AddCircle(scale * 0.5f, scale * 0.5f, scale * 0.35f);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Horizontal:
                                fillPath.MoveTo(0, scale * 0.5f);
                                fillPath.LineTo(scale, scale * 0.5f);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Vertical:
                                fillPath.MoveTo(scale * 0.5f, 0);
                                fillPath.LineTo(scale * 0.5f, scale);
                                paintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Bitmap:
                                paintFill.Style = SKPaintStyle.Fill;
                                var image = GetImage(symbolCache, vectorStyle.Fill.BitmapId);
                                if (image != null)
                                    paintFill.Shader = image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                                break;
                            case FillStyle.BitmapRotated:
                                paintFill.Style = SKPaintStyle.Fill;
                                image = GetImage(symbolCache, vectorStyle.Fill.BitmapId);
                                if (image != null)
                                    paintFill.Shader = image.ToShader(SKShaderTileMode.Repeat,
                                        SKShaderTileMode.Repeat,
                                        SKMatrix.MakeRotation((float)(viewport.Rotation * System.Math.PI / 180.0f), image.Width >> 1, image.Height >> 1));
                                break;
                            default:
                                paintFill.PathEffect = null;
                                break;
                        }

                        // Do this, because if not, path isn't filled complete
                        using (new SKAutoCanvasRestore(canvas))
                        {
                            canvas.ClipPath(path);
                            var bounds = path.Bounds;
                            // Make sure, that the brush starts with the correct position
                            var inflate = ((int)path.Bounds.Width * 0.3f / scale) * scale;
                            bounds.Inflate(inflate, inflate);
                            // Draw rect with bigger size, which is clipped by path
                            canvas.DrawRect(bounds, paintFill);
                        }
                    }

                    if (vectorStyle.Outline != null)
                    {
                        using (var paintStroke = new SKPaint {IsAntialias = true})
                        {
                            paintStroke.Style = SKPaintStyle.Stroke;
                            paintStroke.StrokeWidth = lineWidth;
                            paintStroke.Color = lineColor.ToSkia(opacity);
                            paintStroke.StrokeCap = strokeCap.ToSkia();
                            paintStroke.StrokeJoin = strokeJoin.ToSkia();
                            paintStroke.StrokeMiter = strokeMiterLimit;
                            if (strokeStyle != PenStyle.Solid)
                                paintStroke.PathEffect = strokeStyle.ToSkia(lineWidth, dashArray);
                            else
                                paintStroke.PathEffect = null;
                            canvas.DrawPath(path, paintStroke);
                        }
                    }
                }
            }
        }

        private static SKImage GetImage(SymbolCache symbolCache, int bitmapId)
        {
            var bitmapInfo = symbolCache.GetOrCreate(bitmapId);
            if (bitmapInfo.Type == BitmapType.Bitmap)
                return bitmapInfo.Bitmap;
            if (bitmapInfo.Type == BitmapType.Sprite)
            {
                var sprite = bitmapInfo.Sprite;
                if (sprite.Data == null)
                {
                    var bitmapAtlas = symbolCache.GetOrCreate(sprite.Atlas);
                    sprite.Data = bitmapAtlas.Bitmap.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                        sprite.Y + sprite.Height));
                }
                return (SKImage)sprite.Data;
            }
            return null;
        }
    }
}