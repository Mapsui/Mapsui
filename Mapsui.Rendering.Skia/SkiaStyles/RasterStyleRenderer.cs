using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia;

public class RasterStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, IRenderCache renderCache, long currentIteration)
    {
        try
        {
            var rasterFeature = feature as RasterFeature;
            var raster = rasterFeature?.Raster;

            var opacity = (float)(layer.Opacity * style.Opacity);

            if (raster == null)
                return false;

            if (style is not RasterStyle)
                return false;

            renderCache.TileCache.UpdateCache(currentIteration);

            if (renderCache.TileCache.GetOrCreate(raster, currentIteration) is not BitmapInfo bitmapInfo)
                return false;

            var extent = feature.Extent;

            if (extent == null)
                return false;

            canvas.Save();

            if (viewport.IsRotated())
            {
                var priorMatrix = canvas.TotalMatrix;

                var matrix = CreateRotationMatrix(viewport, extent, priorMatrix);

                canvas.SetMatrix(matrix);

                var destination = new SKRect(0.0f, 0.0f, (float)extent.Width, (float)extent.Height);

                switch (bitmapInfo.Type)
                {
                    case BitmapType.Bitmap:
                        BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap!, destination, opacity);
                        break;
                    case BitmapType.Picture:
                        PictureRenderer.Draw(canvas, bitmapInfo.Picture!, destination, opacity);
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
                        BitmapRenderer.Draw(canvas, bitmapInfo.Bitmap!, RoundToPixel(destination), opacity);
                        break;
                    case BitmapType.Picture:
                        PictureRenderer.Draw(canvas, bitmapInfo.Picture!, RoundToPixel(destination), opacity);
                        break;
                }
            }

            canvas.Restore();
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

        return true;
    }

    private static SKMatrix CreateRotationMatrix(Viewport viewport, MRect rect, SKMatrix priorMatrix)
    {
        // The front-end sets up the canvas with a matrix based on screen scaling (e.g. retina).
        // We need to retain that effect by combining our matrix with the incoming matrix.

        // We'll create four matrices in addition to the incoming matrix. They perform the
        // zoom scale, focal point offset, user rotation and finally, centering in the screen.

        var userRotation = SKMatrix.CreateRotationDegrees((float)viewport.Rotation);
        var focalPointOffset = SKMatrix.CreateTranslation(
            (float)(rect.Left - viewport.CenterX),
            (float)(viewport.CenterY - rect.Top));
        var zoomScale = SKMatrix.CreateScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
        var centerInScreen = SKMatrix.CreateTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

        // We'll concatenate them like so: incomingMatrix * centerInScreen * userRotation * zoomScale * focalPointOffset

        var matrix = SKMatrix.Concat(zoomScale, focalPointOffset);
        matrix = SKMatrix.Concat(userRotation, matrix);
        matrix = SKMatrix.Concat(centerInScreen, matrix);
        matrix = SKMatrix.Concat(priorMatrix, matrix);

        return matrix;
    }

    private static SKRect WorldToScreen(Viewport viewport, MRect rect)
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
