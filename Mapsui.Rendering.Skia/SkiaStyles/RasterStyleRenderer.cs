using System;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class RasterStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long currentIteration)
        {
            try
            {
                var rasterFeature = feature as RasterFeature;
                var raster = rasterFeature?.Raster;

                if (raster == null)
                    return false;

                if (!(style is RasterStyle rasterStyle))
                    return false;

                rasterStyle.UpdateCache(currentIteration);

                BitmapInfo? bitmapInfo;

                if (!rasterStyle.TileCache.Keys.Contains(raster))
                {
                    bitmapInfo = BitmapHelper.LoadBitmap(raster.Data);
                    rasterStyle.TileCache[raster] = bitmapInfo;
                }
                else
                {
                    bitmapInfo = (BitmapInfo?)rasterStyle.TileCache[raster];
                }

                if (bitmapInfo == null || bitmapInfo.Bitmap == null)
                    return false;

                bitmapInfo.IterationUsed = currentIteration;
                rasterStyle.TileCache[raster] = bitmapInfo;

                var extent = feature.Extent;

                if (extent == null)
                    return false;

                canvas.Save();

                var scale = CreateMatrix(canvas, viewport, extent, bitmapInfo.Width);
                var destination = new SKRect(0, 0, bitmapInfo.Width, bitmapInfo.Height);
                var opacity = (float)(layer.Opacity * style.Opacity);

                BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, destination, opacity);

                canvas.Restore();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }

            return true;
        }

        private float CreateMatrix(SKCanvas canvas, IReadOnlyViewport viewport, MRect extent, float tileSize)
        {
            var destinationTopLeft = viewport.WorldToScreen(extent.TopLeft);
            var destinationTopRight = viewport.WorldToScreen(extent.TopRight);

            var dx = destinationTopRight.X - destinationTopLeft.X;
            var dy = destinationTopRight.Y - destinationTopLeft.Y;

            var scale = (float)(Math.Sqrt(dx * dx + dy * dy) / tileSize);

            canvas.Translate((float)destinationTopLeft.X, (float)destinationTopLeft.Y);

            if (viewport.IsRotated)
            {
                canvas.RotateDegrees((float)viewport.Rotation);
            }

            canvas.Scale(scale);

            return scale;
        }
    }
}
