using System;
using Mapsui.Extensions;
using SkiaSharp;
using SKCanvas = SkiaSharp.SKCanvas;

namespace Mapsui.Rendering.Skia.Extensions
{
    public static class ViewportExtensions
    {
        public static SKMatrix ToSKMatrix(this IViewport viewport)
        {
            var mapCenterX = (float)viewport.Width * 0.5f;
            var mapCenterY = (float)viewport.Height * 0.5f;
            var invertedResolution = 1f / (float)viewport.Resolution;

            var matrix = SKMatrix.CreateScale(invertedResolution, invertedResolution, mapCenterX, mapCenterY);
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(1, -1, 0, -mapCenterY)); // As a consequence images will be up side down :(
            if (viewport.IsRotated()) matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)-viewport.Rotation));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)-viewport.CenterX, (float)-viewport.CenterY));
            return matrix;
        }

         /// <summary> Converts the Extent of the Viewport to a SKRect </summary>
        /// <param name="viewport">viewport</param>
        /// <returns>SkRect</returns>
        public static SKRect ToSkRect(this IReadOnlyViewport viewport)
        {
            return viewport.WorldToScreen(viewport.GetExtent()).ToSkia();
        }

        /// <summary>
       /// To Viewport that has the same scale as the canvas
       /// </summary>
       /// <param name="viewport"></param>
       /// <param name="canvas"></param>
       /// <returns></returns>
        public static IReadOnlyViewport ToCanvasViewport(this IReadOnlyViewport viewport, SKCanvas canvas)
        {
            var multiplier = 1.0 / Math.Min(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY);
            var result = new Viewport(viewport);
            result.SetSize(result.Width * multiplier, result.Height * multiplier);
            result.SetCenter(viewport.CenterX - viewport.Width * viewport.Resolution / 2, viewport.CenterY - viewport.Height * viewport.Resolution / 2);
            return result;
        } 
    }
}
