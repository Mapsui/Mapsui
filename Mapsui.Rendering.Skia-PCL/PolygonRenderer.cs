using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal static class PolygonRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, IGeometry geometry,
            float opacity, SymbolCache symbolCache = null)
        {
            if (style is LabelStyle)
            {
                var worldCenter = geometry.GetBoundingBox().GetCentroid();
                var center = viewport.WorldToScreen(worldCenter);
                LabelRenderer.Draw(canvas, (LabelStyle)style, feature, (float)center.X, (float)center.Y, opacity);
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

                if (vectorStyle != null)
                {
                    lineWidth = (float)vectorStyle.Outline.Width;
                    lineColor = vectorStyle.Outline.Color;
                    strokeCap = vectorStyle.Outline.PenStrokeCap;
                    strokeJoin = vectorStyle.Outline.StrokeJoin;
                    strokeMiterLimit = vectorStyle.Outline.StrokeMiterLimit;
                    strokeStyle = vectorStyle.Outline.PenStyle;
                    dashArray = vectorStyle.Outline.DashArray;

                    fillColor = vectorStyle.Fill?.Color;
                }

                using (var path = polygon.ToSkiaPath(viewport, canvas.LocalClipBounds, lineWidth))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;

                        // Is there a FillStyle?
                        if (vectorStyle.Fill?.FillStyle == FillStyle.Solid)
                        {
                            paint.StrokeWidth = lineWidth;
                            paint.Style = SKPaintStyle.Fill;
                            paint.Color = fillColor.ToSkia(opacity);
                            canvas.DrawPath(path, paint);
                        }
                        else
                        {
                            paint.StrokeWidth = 1;
                            paint.Style = SKPaintStyle.Stroke;
                            paint.Color = fillColor.ToSkia(opacity);
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
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.DiagonalCross:
                                    fillPath.MoveTo(scale, scale);
                                    fillPath.LineTo(0, 0);
                                    fillPath.MoveTo(0, scale);
                                    fillPath.LineTo(scale, 0);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.BackwardDiagonal:
                                    fillPath.MoveTo(0, scale);
                                    fillPath.LineTo(scale, 0);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.ForwardDiagonal:
                                    fillPath.MoveTo(scale, scale);
                                    fillPath.LineTo(0, 0);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.Dotted:
                                    paint.Style = SKPaintStyle.StrokeAndFill;
                                    fillPath.AddCircle(scale * 0.5f, scale * 0.5f, scale * 0.35f);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.Horizontal:
                                    fillPath.MoveTo(0, scale * 0.5f);
                                    fillPath.LineTo(scale, scale * 0.5f);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.Vertical:
                                    fillPath.MoveTo(scale * 0.5f, 0);
                                    fillPath.LineTo(scale * 0.5f, scale);
                                    paint.PathEffect = SKPathEffect.Create2DPath(matrix, fillPath);
                                    break;
                                case FillStyle.Bitmap:
                                    paint.Style = SKPaintStyle.Fill;
                                    paint.Shader = symbolCache.GetOrCreate(vectorStyle.Fill.BitmapId).Bitmap.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                                    break;
                                case FillStyle.BitmapRotated:
                                    paint.Style = SKPaintStyle.Fill;
                                    SKImage bitmap = symbolCache.GetOrCreate(vectorStyle.Fill.BitmapId).Bitmap;
                                    paint.Shader = bitmap.ToShader(SKShaderTileMode.Repeat,
                                        SKShaderTileMode.Repeat,
                                        SKMatrix.MakeRotation((float)(viewport.Rotation * System.Math.PI / 180.0f), bitmap.Width >> 1, bitmap.Height >> 1));
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
                                canvas.DrawRect(bounds, paint);
                            }
                        }
                    }

                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;
                        paint.Style = SKPaintStyle.Stroke;
                        paint.StrokeWidth = lineWidth;
                        paint.Color = lineColor.ToSkia(opacity);
                        paint.StrokeCap = strokeCap.ToSkia();
                        paint.StrokeJoin = strokeJoin.ToSkia();
                        paint.StrokeMiter = strokeMiterLimit;
                        if (strokeStyle != PenStyle.Solid)
                            paint.PathEffect = strokeStyle.ToSkia(lineWidth, dashArray);
                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }
    }
}