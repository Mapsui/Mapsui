using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    static class PointRenderer
    {
        private const float HalfWidth = (float) SymbolStyle.DefaultWidth/2;
        private const float HalfHeight = (float) SymbolStyle.DefaultHeight/2;

        // todo: 
        // try to remove the feature argument. LabelStyle should already contain the feature specific text
        // The visible feature iterator should create this LabelStyle
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature, 
            IGeometry geometry, IDictionary<int, SKBitmapInfo> symbolBitmapCache)
        {
            var point = geometry as Point;
            var destination = viewport.WorldToScreen(point);

            if (style is LabelStyle)              // case 1) LabelStyle
            {
                LabelRenderer.Draw(canvas, (LabelStyle) style, feature, (float) destination.X, (float) destination.Y);
            }
            else if (style is SymbolStyle)
            {
                var symbolStyle = (SymbolStyle)style;

                if ( symbolStyle.BitmapId >= 0)   // case 2) Bitmap Style
                {
                    DrawPointWithBitmapStyle(canvas, symbolStyle, destination, symbolBitmapCache);
                }
                else                              // case 3) SymbolStyle without bitmap
                {
                    DrawPointWithSymbolStyle(canvas, symbolStyle, destination, symbolStyle.SymbolType);
                }
            }
            else if (style is VectorStyle)        // case 4) VectorStyle
            {
                DrawPointWithVectorStyle(canvas, (VectorStyle) style, destination);
            }
            else
            {
                throw new Exception($"Style of type '{style.GetType()}' is not supported for points");
            }
        }

        private static void DrawPointWithSymbolStyle(SKCanvas canvas, SymbolStyle style,
            Point destination, SymbolType symbolType = SymbolType.Ellipse)
        {
            canvas.Save();
            canvas.Translate((float)destination.X,(float)destination.Y);
            canvas.Scale((float)style.SymbolScale, (float)style.SymbolScale);
            DrawPointWithVectorStyle(canvas, style, symbolType);
            canvas.Restore();
        }

        private static void DrawPointWithVectorStyle(SKCanvas canvas, VectorStyle vectorStyle,
            Point destination, SymbolType symbolType = SymbolType.Ellipse)
        {
            canvas.Save();
            canvas.Translate((float)destination.X, (float)destination.Y);
            DrawPointWithVectorStyle(canvas, vectorStyle, symbolType);
            canvas.Restore();
        }

        private static void DrawPointWithVectorStyle(SKCanvas canvas, VectorStyle vectorStyle,
            SymbolType symbolType = SymbolType.Ellipse)
        {
            var fillPaint = new SKPaint
            {
                Color = vectorStyle.Fill.Color.ToSkia(),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var linePaint = (vectorStyle.Outline == null) ? null : new SKPaint
            {
                Color = vectorStyle.Outline.Color.ToSkia(),
                StrokeWidth = (float) vectorStyle.Outline.Width,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            if (symbolType == SymbolType.Rectangle)
            {
                var rect = new SKRect(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight);
                DrawRect(canvas, rect, fillPaint, linePaint);
            }
            else if (symbolType == SymbolType.Ellipse)
            {

                DrawCircle(canvas, 0, 0, HalfWidth, fillPaint, linePaint);
            }
        }
        
        private static void DrawCircle(SKCanvas canvas, float x, float y, float radius, SKPaint fillColor,
            SKPaint lineColor)
        {
            if ((fillColor != null) && fillColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, fillColor);
            if ((lineColor != null) && lineColor.Color.Alpha != 0) canvas.DrawCircle(x, y, radius, lineColor);
        }

        private static void DrawRect(SKCanvas canvas, SKRect rect, SKPaint fillColor, SKPaint lineColor)
        {
            if ((fillColor != null) && fillColor.Color.Alpha != 0) canvas.DrawRect(rect, fillColor);
            if ((lineColor != null) && lineColor.Color.Alpha != 0) canvas.DrawRect(rect, lineColor);
        }

        private static void DrawPointWithBitmapStyle(SKCanvas canvas, SymbolStyle symbolStyle, Point destination,
            IDictionary<int, SKBitmapInfo> symbolBitmapCache)
        {
            var stream = BitmapRegistry.Instance.Get(symbolStyle.BitmapId);
            stream.Position = 0;
            SKBitmapInfo textureInfo;
            if (!symbolBitmapCache.Keys.Contains(symbolStyle.BitmapId))
            {
                textureInfo = BitmapHelper.LoadTexture(BitmapRegistry.Instance.Get(symbolStyle.BitmapId));
                symbolBitmapCache[symbolStyle.BitmapId] = textureInfo;
            }
            else
            {
                textureInfo = symbolBitmapCache[symbolStyle.BitmapId];
            }

            BitmapHelper.RenderTexture(canvas, textureInfo.Bitmap,
                (float) destination.X, (float) destination.Y,
                (float) symbolStyle.SymbolRotation,
                (float) symbolStyle.SymbolOffset.X, (float) symbolStyle.SymbolOffset.Y,
                opacity: (float) symbolStyle.Opacity, scale: (float) symbolStyle.SymbolScale);
        }
    }
}