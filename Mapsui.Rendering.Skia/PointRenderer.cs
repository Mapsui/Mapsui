using System;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    static class PointRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature, 
            IGeometry geometry, SymbolCache symbolCache, float opacity)
        {
            var point = geometry as Point;
            var destination = viewport.WorldToScreen(point);

            if (style is LabelStyle labelStyle)    // case 1) LabelStyle
            {
                LabelRenderer.Draw(canvas, labelStyle, feature, (float) destination.X, (float) destination.Y, 
                    opacity);
            }
            else if (style is SymbolStyle)
            {
                var symbolStyle = (SymbolStyle)style;

                if ( symbolStyle.BitmapId >= 0)   // case 2) Bitmap Style
                {
                    DrawPointWithBitmapStyle(canvas, symbolStyle, destination, symbolCache, opacity, (float)viewport.Rotation);
                }
                else                              // case 3) SymbolStyle without bitmap
                {
                    DrawPointWithSymbolStyle(canvas, symbolStyle, destination, opacity, symbolStyle.SymbolType, (float)viewport.Rotation);
                }
            }
            else if (style is VectorStyle)        // case 4) VectorStyle
            {
                DrawPointWithVectorStyle(canvas, (VectorStyle) style, destination, opacity);
            }
            else
            {
                throw new Exception($"Style of type '{style.GetType()}' is not supported for points");
            }
        }

        private static void DrawPointWithSymbolStyle(SKCanvas canvas, SymbolStyle style,
            Point destination, float opacity, SymbolType symbolType, float mapRotation)
        {
            canvas.Save();
            canvas.Translate((float)destination.X, (float)destination.Y);
            canvas.Scale((float)style.SymbolScale, (float)style.SymbolScale);
            if (style.SymbolOffset.IsRelative)
                canvas.Translate((float)(SymbolStyle.DefaultWidth * style.SymbolOffset.X), (float)(-SymbolStyle.DefaultWidth * style.SymbolOffset.Y));
            else
                canvas.Translate((float) style.SymbolOffset.X, (float) -style.SymbolOffset.Y);
            if (style.SymbolRotation != 0)
            {
                var rotation = (float)style.SymbolRotation;
                if (style.RotateWithMap) rotation += mapRotation;
                canvas.RotateDegrees(rotation);
            }

            DrawPointWithVectorStyle(canvas, style, opacity, symbolType);
            canvas.Restore();
        }

        private static void DrawPointWithVectorStyle(SKCanvas canvas, VectorStyle vectorStyle,
            Point destination, float opacity, SymbolType symbolType = SymbolType.Ellipse)
        {
            canvas.Save();
            canvas.Translate((float)destination.X, (float)destination.Y);
            DrawPointWithVectorStyle(canvas, vectorStyle, opacity, symbolType);
            canvas.Restore();
        }

        private static void DrawPointWithVectorStyle(SKCanvas canvas, VectorStyle vectorStyle,
            float opacity, SymbolType symbolType = SymbolType.Ellipse)
        {
            var width = (float)SymbolStyle.DefaultWidth;
            var halfWidth = width / 2;
            var halfHeight = (float)SymbolStyle.DefaultHeight / 2;

            var fillPaint = CreateFillPaint(vectorStyle.Fill, opacity);

            var linePaint = CreateLinePaint(vectorStyle.Outline, opacity);

            switch (symbolType)
            {
                case SymbolType.Ellipse:
                    DrawCircle(canvas, 0, 0, halfWidth, fillPaint, linePaint);
                    break;
                case SymbolType.Rectangle:
                    var rect = new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);
                    DrawRect(canvas, rect, fillPaint, linePaint);
                    break;
                case SymbolType.Triangle:
                    DrawTriangle(canvas, 0, 0, width, fillPaint, linePaint);
                    break;
                default: // Invalid value
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static SKPaint CreateLinePaint(Pen outline, float opacity)
        {
            if (outline == null) return null;

            return new SKPaint
            {
                Color = outline.Color.ToSkia(opacity),
                StrokeWidth = (float) outline.Width,
                StrokeCap = outline.PenStrokeCap.ToSkia(),
                PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint CreateFillPaint(Brush fill, float opacity)
        {
            if (fill == null) return null;

            return new SKPaint
            {
                Color = fill.Color.ToSkia(opacity),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static void DrawCircle(SKCanvas canvas, float x, float y, float radius, SKPaint fillColor,
            SKPaint lineColor)
        {
            if (fillColor != null && fillColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, fillColor);
            if (lineColor != null && lineColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, lineColor);
        }

        private static void DrawRect(SKCanvas canvas, SKRect rect, SKPaint fillColor, SKPaint lineColor)
        {
            if (fillColor != null && fillColor.Color.Alpha != 0) canvas.DrawRect(rect, fillColor);
            if (lineColor != null && lineColor.Color.Alpha != 0) canvas.DrawRect(rect, lineColor);
        }

        /// <summary>
        /// Equilateral triangle of side 'sideLength', centered on the same point as if a circle of diameter 'sideLength' was there
        /// </summary>
        private static void DrawTriangle(SKCanvas canvas, float x, float y, float sideLength, SKPaint fillColor, SKPaint lineColor)
        {
            var altitude = Math.Sqrt(3) / 2.0 * sideLength;
            var inradius = altitude / 3.0;
            var circumradius = 2.0 * inradius;

            var top = new Point(x, y - circumradius);
            var left = new Point(x + sideLength * -0.5, y + inradius);
            var right = new Point(x + sideLength * 0.5, y + inradius);

            var path = new SKPath();
            path.MoveTo((float)top.X, (float)top.Y);
            path.LineTo((float)left.X, (float)left.Y);
            path.LineTo((float)right.X, (float)right.Y);
            path.Close();

            if ((fillColor != null) && fillColor.Color.Alpha != 0) canvas.DrawPath(path, fillColor);
            if ((lineColor != null) && lineColor.Color.Alpha != 0) canvas.DrawPath(path, lineColor);
        }

        private static void DrawPointWithBitmapStyle(SKCanvas canvas, SymbolStyle symbolStyle, Point destination,
            SymbolCache symbolCache, float opacity, float mapRotation)
        {
            var bitmap = symbolCache.GetOrCreate(symbolStyle.BitmapId);

            // Calc offset (relative or absolute)
            var offsetX = symbolStyle.SymbolOffset.IsRelative ? bitmap.Width * symbolStyle.SymbolOffset.X : symbolStyle.SymbolOffset.X;
            var offsetY = symbolStyle.SymbolOffset.IsRelative ? bitmap.Height * symbolStyle.SymbolOffset.Y : symbolStyle.SymbolOffset.Y;

            var rotation = (float) symbolStyle.SymbolRotation;
            if (symbolStyle.RotateWithMap) rotation += mapRotation;

            switch (bitmap.Type)
            {
                case BitmapType.Bitmap:
                    BitmapHelper.RenderBitmap(canvas, bitmap.Bitmap,
                        (float) destination.X, (float) destination.Y,
                        rotation,
                        (float) offsetX, (float) offsetY,
                        opacity: opacity, scale: (float) symbolStyle.SymbolScale);
                    break;
                case BitmapType.Svg:
                    BitmapHelper.RenderSvg(canvas, bitmap.Svg,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
                case BitmapType.Sprite:
                    var sprite = bitmap.Sprite;
                    if (sprite.Data == null)
                    {
                        var bitmapAtlas = symbolCache.GetOrCreate(sprite.Atlas);
                        sprite.Data = bitmapAtlas.Bitmap.Subset(new SKRectI(sprite.X, sprite.Y, sprite.X + sprite.Width,
                            sprite.Y + sprite.Height));
                    }
                    BitmapHelper.RenderBitmap(canvas, (SKImage)sprite.Data,
                        (float)destination.X, (float)destination.Y,
                        rotation,
                        (float)offsetX, (float)offsetY,
                        opacity: opacity, scale: (float)symbolStyle.SymbolScale);
                    break;
            }
        }
    }
}