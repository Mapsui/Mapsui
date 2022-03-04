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

                var opacity = (float)(layer.Opacity * style.Opacity);

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

                if (viewport.IsRotated)
                {
                    var priorMatrix = canvas.TotalMatrix;

                    var matrix = CreateRotationMatrix(viewport, extent, priorMatrix);

                    canvas.SetMatrix(matrix);

                    var destination = new SKRect(0.0f, 0.0f, (float)extent.Width, (float)extent.Height);

                    BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, destination, opacity);

                    canvas.SetMatrix(priorMatrix);
                }
                else
                {
                    var destination = WorldToScreen(viewport, extent);
                    BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, RoundToPixel(destination), opacity);
                }

                canvas.Restore();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }

            return true;
        }

        private static SKMatrix CreateRotationMatrix(IReadOnlyViewport viewport, MRect rect, SKMatrix priorMatrix)
        {
            // The front-end sets up the canvas with a matrix based on screen scaling (e.g. retina).
            // We need to retain that effect by combining our matrix with the incoming matrix.

            // We'll create four matrices in addition to the incoming matrix. They perform the
            // zoom scale, focal point offset, user rotation and finally, centering in the screen.

            var userRotation = SKMatrix.CreateRotationDegrees((float)viewport.Rotation);
            var focalPointOffset = SKMatrix.CreateTranslation(
                (float)(rect.Left - viewport.Center.X),
                (float)(viewport.Center.Y - rect.Top));
            var zoomScale = SKMatrix.CreateScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
            var centerInScreen = SKMatrix.CreateTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

            // We'll concatenate them like so: incomingMatrix * centerInScreen * userRotation * zoomScale * focalPointOffset

            var matrix = SKMatrix.Concat(zoomScale, focalPointOffset);
            matrix = SKMatrix.Concat(userRotation, matrix);
            matrix = SKMatrix.Concat(centerInScreen, matrix);
            matrix = SKMatrix.Concat(priorMatrix, matrix);

            return matrix;
        }

        private static SKRect WorldToScreen(IReadOnlyViewport viewport, MRect rect)
        {
            var first = viewport.WorldToScreen(rect.Min.X, rect.Min.Y);
            var second = viewport.WorldToScreen(rect.Max.X, rect.Max.Y);
            return new SKRect
            (
                (float)Math.Min(first.X, second.X),
                (float)Math.Min(first.Y, second.Y),
                (float)Math.Max(first.X, second.X),
                (float)Math.Max(first.Y, second.Y)
            );
        }

        private static SKRect RoundToPixel(SKRect boundingBox)
        {
            return new SKRect(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}
