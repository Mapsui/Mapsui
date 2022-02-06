using System;
using System.Collections.Generic;
using Mapsui.Logging;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class RasterRenderer
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature, MRaster? raster, float opacity, IDictionary<object, BitmapInfo?> tileCache, long currentIteration)
        {
            try
            {
                if (raster == null)
                    return;

                BitmapInfo? bitmapInfo;

                if (!tileCache.Keys.Contains(raster))
                {
                    bitmapInfo = BitmapHelper.LoadBitmap(raster.Data);
                    tileCache[raster] = bitmapInfo;
                }
                else
                {
                    bitmapInfo = tileCache[raster];
                }

                if (bitmapInfo == null)
                    return;

                bitmapInfo.IterationUsed = currentIteration;
                tileCache[raster] = bitmapInfo;

                var extent = feature.Extent;

                if (extent == null)
                    return;

                if (bitmapInfo.Bitmap == null && bitmapInfo.Picture == null)
                    return;

                if (viewport.IsRotated)
                {
                    var priorMatrix = canvas.TotalMatrix;

                    var matrix = CreateRotationMatrix(viewport, extent, priorMatrix);

                    canvas.SetMatrix(matrix);

                    var destination = new SKRect(0.0f, 0.0f, (float)extent.Width, (float)extent.Height);

                    switch (bitmapInfo.Type)
                    {
                        case BitmapType.Bitmap:
                            BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, RoundToPixel(destination), opacity);
                            break;
                        case BitmapType.Picture:
                            PictureRenderer.Draw(canvas, bitmapInfo.Picture, RoundToPixel(destination), opacity);
                            break;
                    }

                    canvas.SetMatrix(priorMatrix);
                }
                else
                {
                    var destination = WorldToScreen(viewport, extent);
                    switch (bitmapInfo.Type)
                    {
                        case BitmapType.Bitmap:
                            BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, RoundToPixel(destination), opacity);
                            break;
                        case BitmapType.Picture:
                            PictureRenderer.Draw(canvas, bitmapInfo.Picture, RoundToPixel(destination), opacity);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        internal static SKMatrix CreateRotationMatrix(IReadOnlyViewport viewport, MRect rect, SKMatrix priorMatrix)
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

        internal static SKRect WorldToScreen(IReadOnlyViewport viewport, MRect rect)
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

        internal static SKRect RoundToPixel(SKRect boundingBox)
        {
            return new SKRect(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}