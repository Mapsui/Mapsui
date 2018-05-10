using System;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PolygonRenderer
    {
        private static readonly SKPaint PaintStroke = new SKPaint { IsAntialias = true };
        private static readonly SKPaint PaintFill = new SKPaint { IsAntialias = true };

        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity, SymbolCache symbolCache = null)
        {
            if (style is LabelStyle)
            {
                var worldCenter = geometry.GetBoundingBox().GetCentroid();
                var center = viewport.WorldToScreen(worldCenter);
                LabelRenderer.Draw(canvas, (LabelStyle)style, feature, (float)center.X, (float)center.Y, opacity);
            }
            else if (style is StyleCollection styleCollection)
            {
                foreach (var s in styleCollection)
                {
                    Draw(canvas, viewport, s, feature, geometry, opacity, symbolCache);
                }
            }
            else
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

                var vectorStyle = style as VectorStyle;

                if (vectorStyle != null && vectorStyle.Outline != null)
                {
                    lineWidth = (float)vectorStyle.Outline.Width;
                    lineColor = vectorStyle.Outline.Color;
                    strokeCap = vectorStyle.Outline.PenStrokeCap;
                    strokeJoin = vectorStyle.Outline.StrokeJoin;
                    strokeMiterLimit = vectorStyle.Outline.StrokeMiterLimit;
                    strokeStyle = vectorStyle.Outline.PenStyle;
                    dashArray = vectorStyle.Outline.DashArray;
                }

                if (vectorStyle != null && vectorStyle.Fill != null)
                {
                    fillColor = vectorStyle.Fill?.Color;
                }

                using (var path = polygon.ToSkiaPath(viewport, canvas.LocalClipBounds, lineWidth))
                {
                    // Is there a FillStyle?
                    if (vectorStyle.Fill?.FillStyle == FillStyle.Solid)
                    {
                        PaintFill.StrokeWidth = 0;
                        PaintFill.Style = SKPaintStyle.Fill;
                        PaintFill.PathEffect = null;
                        PaintFill.Shader = null;
                        PaintFill.Color = fillColor.ToSkia(opacity);
                        canvas.DrawPath(path, PaintFill);
                    }
                    else
                    {
                        PaintFill.StrokeWidth = 1;
                        PaintFill.Style = SKPaintStyle.Stroke;
                        PaintFill.Shader = null;
                        PaintFill.Color = fillColor.ToSkia(opacity);
                        float scale = 10.0f;
                        SKPath fillPath = new SKPath();
                        SKMatrix matrix = SKMatrix.MakeScale(scale, scale);

                        switch (vectorStyle.Fill?.FillStyle)
                        {
                            case FillStyle.Cross:
                                fillPath.MoveTo(scale * 0.8f, scale * 0.8f);
                                fillPath.LineTo(0, 0);
                                fillPath.MoveTo(0, scale * 0.8f);
                                fillPath.LineTo(scale * 0.8f, 0);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.DiagonalCross:
                                fillPath.MoveTo(scale, scale);
                                fillPath.LineTo(0, 0);
                                fillPath.MoveTo(0, scale);
                                fillPath.LineTo(scale, 0);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.BackwardDiagonal:
                                fillPath.MoveTo(0, scale);
                                fillPath.LineTo(scale, 0);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.ForwardDiagonal:
                                fillPath.MoveTo(scale, scale);
                                fillPath.LineTo(0, 0);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Dotted:
                                PaintFill.Style = SKPaintStyle.StrokeAndFill;
                                fillPath.AddCircle(scale * 0.5f, scale * 0.5f, scale * 0.35f);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Horizontal:
                                fillPath.MoveTo(0, scale * 0.5f);
                                fillPath.LineTo(scale, scale * 0.5f);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Vertical:
                                fillPath.MoveTo(scale * 0.5f, 0);
                                fillPath.LineTo(scale * 0.5f, scale);
                                PaintFill.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                break;
                            case FillStyle.Bitmap:
                                PaintFill.Style = SKPaintStyle.Fill;
                                var image = GetImage(symbolCache, vectorStyle.Fill.BitmapId);
                                if (image != null)
                                    PaintFill.Shader = image.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                                break;
                            case FillStyle.BitmapRotated:
                                PaintFill.Style = SKPaintStyle.Fill;
                                image = GetImage(symbolCache, vectorStyle.Fill.BitmapId);
                                if (image != null)
                                    PaintFill.Shader = image.ToShader(SKShaderTileMode.Repeat,
                                        SKShaderTileMode.Repeat,
                                        SKMatrix.MakeRotation((float)(viewport.Rotation * System.Math.PI / 180.0f), image.Width >> 1, image.Height >> 1));
                                break;
                            default:
                                PaintFill.PathEffect = null;
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
                            canvas.DrawRect(bounds, PaintFill);
                        }
                    }

                    if (vectorStyle.Outline != null)
                    {
                        PaintStroke.Style = SKPaintStyle.Stroke;
                        PaintStroke.StrokeWidth = lineWidth;
                        PaintStroke.Color = lineColor.ToSkia(opacity);
                        PaintStroke.StrokeCap = strokeCap.ToSkia();
                        PaintStroke.StrokeJoin = strokeJoin.ToSkia();
                        PaintStroke.StrokeMiter = strokeMiterLimit;
                        if (strokeStyle != PenStyle.Solid)
                            PaintStroke.PathEffect = strokeStyle.ToSkia(lineWidth, dashArray);
                        else
                            PaintStroke.PathEffect = null;

                        canvas.DrawPath(path, PaintStroke);
                    }
                }
            }
        }

        private static SKImage GetImage(SymbolCache symbolCache, int bitmapId)
        {
            var bitmapInfo = symbolCache.GetOrCreate(bitmapId);
            if (bitmapInfo.Type == BitmapType.Bitmap)
                return bitmapInfo.Bitmap;
            else if (bitmapInfo.Type == BitmapType.Sprite)
            {
                var sprite = bitmapInfo.Sprite;
                if (sprite.Data == null)
                {
                    var bitmapAtlas = symbolCache.GetOrCreate(sprite.Atlas);
                    sprite.Data = bitmapAtlas.Bitmap.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                        sprite.Y + sprite.Height));
                }
                return ((SKImage)sprite.Data);
            }

            return null;
        }
    }
}