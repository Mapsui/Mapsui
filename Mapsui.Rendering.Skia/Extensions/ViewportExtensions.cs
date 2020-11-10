﻿using SkiaSharp;

namespace Mapsui.Rendering.Skia
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
            if (viewport.IsRotated) matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)-viewport.Rotation));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)-viewport.Center.X, (float)-viewport.Center.Y));
            return matrix;
        }
    }
}
