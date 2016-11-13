using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class RasterRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature,
            IDictionary<object, SKBitmapInfo> skBitmapCache, long currentIteration)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;

                SKBitmapInfo textureInfo;

                if (!skBitmapCache.Keys.Contains(raster))
                {
                    textureInfo = BitmapHelper.LoadTexture(raster.Data);
                    skBitmapCache[raster] = textureInfo;
                }
                else
                {
                    textureInfo = skBitmapCache[raster];
                }

                textureInfo.IterationUsed = currentIteration;
                skBitmapCache[raster] = textureInfo;
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                BitmapHelper.RenderRaster(canvas, textureInfo.Bitmap, RoundToPixel(destination).ToSkia());
            }
            catch (Exception ex)
            {
                Logger.LogDelegate(LogLevel.Error, ex.Message, ex);
            }
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
            (
                Math.Min(first.X, second.X),
                Math.Min(first.Y, second.Y),
                Math.Max(first.X, second.X),
                Math.Max(first.Y, second.Y)
            );
        }

        private static BoundingBox RoundToPixel(BoundingBox boundingBox)
        {
            return new BoundingBox(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}
