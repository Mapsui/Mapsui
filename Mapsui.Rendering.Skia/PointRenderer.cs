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

            var labelStyle = style as LabelStyle;
            if (labelStyle != null)
            {
                var text = labelStyle.GetLabelText(null);
                LabelRenderer.Draw(canvas, labelStyle, text, (float) destination.X, (float) destination.Y);
            }
            else
            {
                var symbolStyle = style as SymbolStyle;

                if (symbolStyle != null)
                {
                    if (symbolStyle.BitmapId >= 0)
                        DrawPointWithSymbolStyle(canvas, symbolStyle, destination, symbolBitmapCache);
                    else
                        DrawPointWithVectorStyle(canvas, (VectorStyle) style, destination, symbolStyle.SymbolType);
                }
                else if (style is VectorStyle)
                    DrawPointWithVectorStyle(canvas, (VectorStyle) style, destination);
            }
        }

        private static void DrawPointWithVectorStyle(SKCanvas canvas, VectorStyle vectorStyle,
            Point destination, SymbolType symbolType = SymbolType.Ellipse)
        {
            canvas.Save();

            canvas.Translate((float) destination.X, (float) destination.Y);


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

            canvas.Restore();
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

        private static void DrawPointWithSymbolStyle(SKCanvas canvas, SymbolStyle symbolStyle, Point destination,
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