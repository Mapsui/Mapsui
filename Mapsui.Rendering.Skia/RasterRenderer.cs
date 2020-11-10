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
		public static void Draw (SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, IFeature feature,
            float opacity, IDictionary<object, BitmapInfo> tileCache, long currentIteration)
		{
		    try
		    {
		        var raster = (IRaster)feature.Geometry;

		        BitmapInfo bitmapInfo;

		        if (!tileCache.Keys.Contains(raster))
		        {
		            bitmapInfo = BitmapHelper.LoadBitmap(raster.Data);
		            tileCache[raster] = bitmapInfo;
		        }
		        else
		        {
		            bitmapInfo = tileCache[raster];
		        }

		        bitmapInfo.IterationUsed = currentIteration;
		        tileCache[raster] = bitmapInfo;

		        var boundingBox = feature.Geometry.BoundingBox;

		        if (viewport.IsRotated)
		        {
		            var priorMatrix = canvas.TotalMatrix;

		            var matrix = CreateRotationMatrix(viewport, boundingBox, priorMatrix);

		            canvas.SetMatrix(matrix);

		            var destination = new BoundingBox(0.0, 0.0, boundingBox.Width, boundingBox.Height);

                    BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, destination.ToSkia(), opacity);

		            canvas.SetMatrix(priorMatrix);
		        }
		        else
		        {
		            var destination = WorldToScreen(viewport, feature.Geometry.BoundingBox);
		            BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap, RoundToPixel(destination).ToSkia(), opacity);
                }
		    }
			catch (Exception ex)
			{
				Logger.Log (LogLevel.Error, ex.Message, ex);
			}
		}

        private static SKMatrix CreateRotationMatrix(IReadOnlyViewport viewport, BoundingBox boundingBox, SKMatrix priorMatrix)
        {
            // The front-end sets up the canvas with a matrix based on screen scaling (e.g. retina).
            // We need to retain that effect by combining our matrix with the incoming matrix.

            // We'll create four matrices in addition to the incoming matrix. They perform the
            // zoom scale, focal point offset, user rotation and finally, centering in the screen.

            var userRotation = SKMatrix.CreateRotationDegrees((float) viewport.Rotation);
            var focalPointOffset = SKMatrix.CreateTranslation(
                (float) (boundingBox.Left - viewport.Center.X),
                (float) (viewport.Center.Y - boundingBox.Top));
            var zoomScale = SKMatrix.CreateScale((float) (1.0 / viewport.Resolution), (float) (1.0 / viewport.Resolution));
            var centerInScreen = SKMatrix.CreateTranslation((float) (viewport.Width / 2.0), (float) (viewport.Height / 2.0));

            // We'll concatenate them like so: incomingMatrix * centerInScreen * userRotation * zoomScale * focalPointOffset

            SKMatrix matrix = SKMatrix.Concat(zoomScale, focalPointOffset);
            matrix = SKMatrix.Concat(userRotation, matrix);
            matrix = SKMatrix.Concat(centerInScreen, matrix);
            matrix = SKMatrix.Concat(priorMatrix, matrix);

            return matrix;
        }

        private static BoundingBox WorldToScreen(IReadOnlyViewport viewport, BoundingBox boundingBox)
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