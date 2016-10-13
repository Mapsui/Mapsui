using System;
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

        public static void Draw(SKCanvas skCanvas, IViewport viewport, IStyle style, IFeature feature,
            IDictionary<int, SKBitmapInfo> _symbolTextureCache)
        {
            var point = feature.Geometry as Point;
            var destination = viewport.WorldToScreen(point);

            if (style is LabelStyle)
            {
                var labelStyle = (LabelStyle) style;
                //LabelRenderer.Draw(labelStyle, labelStyle.GetLabelText(feature), (float)destination.X, (float)destination.Y);
            }
            var symbolStyle = style as SymbolStyle;
            if (symbolStyle != null && symbolStyle.BitmapId >= 0)
                DrawPointWithSymbolStyle(skCanvas, symbolStyle, destination);
            else if (style is VectorStyle)
                DrawPointWithVectorStyle(skCanvas, (VectorStyle) style, destination);
        }

        private static void DrawPointWithVectorStyle(SKCanvas sKCanvas, VectorStyle vectorStyle, Point destination)
        {
            sKCanvas.Save();
            sKCanvas.Translate((float) destination.X, (float) destination.Y);

            var rect = new SKRect(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight);

            sKCanvas.DrawRect(rect, new SKPaint {Color = vectorStyle.Fill.Color.ToSkia(), Style = SKPaintStyle.Fill});
            sKCanvas.DrawRect(rect,
                new SKPaint
                {
                    Color = vectorStyle.Outline.Color.ToSkia(),
                    StrokeWidth = (float) vectorStyle.Outline.Width,
                    Style = SKPaintStyle.Stroke
                });
            sKCanvas.Restore();
        }

        private static void DrawPointWithSymbolStyle(SKCanvas skCanvas, SymbolStyle symbolStyle, Point destination)
        {
            var stream = BitmapRegistry.Instance.Get(symbolStyle.BitmapId);
            stream.Position = 0;

            using (var skStream = new SKManagedStream(stream))
            {
                using (var bitmap = SKBitmap.Decode(skStream))
                {
                    skCanvas.DrawBitmap(bitmap, (float)destination.X, (float)destination.Y);
                }
                //TextureHelper.RenderTexture(textureInfo, (float) destination.X, (float) destination.Y,
                //        (float) symbolStyle.SymbolRotation,
                //        (float) symbolStyle.SymbolOffset.X, (float) symbolStyle.SymbolOffset.Y,
                //        opacity: (float) symbolStyle.Opacity, scale: (float) symbolStyle.SymbolScale);
            }
        }
    }
}