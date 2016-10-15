using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal class PointRenderer
    {
        private const float HalfWidth = (float) SymbolStyle.DefaultWidth/2;
        private const float HalfHeight = (float) SymbolStyle.DefaultHeight/2;

        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature,
            IDictionary<int, SKBitmapInfo> symbolBitmapCache)
        {
            var point = feature.Geometry as Point;
            var destination = viewport.WorldToScreen(point);

            var labelStyle = style as LabelStyle;
            if (labelStyle != null)
            {
                LabelRenderer.Draw(canvas, labelStyle, labelStyle.GetLabelText(feature), 
                    (float) destination.X, (float) destination.Y);
            }
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null && symbolStyle.BitmapId >= 0)
                DrawPointWithSymbolStyle(canvas, symbolStyle, destination, symbolBitmapCache);
            else if (style is VectorStyle)
                DrawPointWithVectorStyle(canvas, (VectorStyle) style, destination);
        }

        private static void DrawPointWithVectorStyle(SKCanvas skCanvas, VectorStyle vectorStyle,
            Point destination)
        {
            skCanvas.Save();
            skCanvas.Translate((float) destination.X, (float) destination.Y);

            var rect = new SKRect(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight);

            skCanvas.DrawRect(rect, new SKPaint {Color = vectorStyle.Fill.Color.ToSkia(), Style = SKPaintStyle.Fill});
            skCanvas.DrawRect(rect,
                new SKPaint
                {
                    Color = vectorStyle.Outline.Color.ToSkia(),
                    StrokeWidth = (float) vectorStyle.Outline.Width,
                    Style = SKPaintStyle.Stroke
                });
            skCanvas.Restore();
        }

        private static void DrawPointWithSymbolStyle(SKCanvas skCanvas, SymbolStyle symbolStyle, Point destination,
            IDictionary<int, SKBitmapInfo> symbolBitmapCache)
        {
            var stream = BitmapRegistry.Instance.Get(symbolStyle.BitmapId);
            stream.Position = 0;
            SKBitmapInfo textureInfo;
            if (!symbolBitmapCache.Keys.Contains(symbolStyle.BitmapId))
            {
                textureInfo = TextureHelper.LoadTexture(BitmapRegistry.Instance.Get(symbolStyle.BitmapId));
                symbolBitmapCache[symbolStyle.BitmapId] = textureInfo;
            }
            else
            {
                textureInfo = symbolBitmapCache[symbolStyle.BitmapId];
            }

            TextureHelper.RenderTexture(skCanvas, textureInfo.Bitmap,
                (float) destination.X, (float) destination.Y,
                (float) symbolStyle.SymbolRotation,
                (float) symbolStyle.SymbolOffset.X, (float) symbolStyle.SymbolOffset.Y,
                opacity: (float) symbolStyle.Opacity, scale: (float) symbolStyle.SymbolScale);
        }
    }
}