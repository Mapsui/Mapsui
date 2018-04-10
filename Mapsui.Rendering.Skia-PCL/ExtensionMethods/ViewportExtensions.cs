using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class ViewportExtensions
    {
        public static SKMatrix ToSKMatrix(this IViewport viewport)
        {
            var mapCenterX = (float)viewport.Width * 0.5f;
            var mapCenterY = (float)viewport.Height * 0.5f;
            var invertedResolution = 1f / (float)viewport.Resolution;

            var matrix = SKMatrix.MakeIdentity();
            SKMatrix.Concat(ref matrix, matrix, SKMatrix.MakeScale(invertedResolution, invertedResolution, mapCenterX, mapCenterY));
            SKMatrix.Concat(ref matrix, matrix, SKMatrix.MakeScale(1, -1, 0, -mapCenterY)); // As a consequence images will be up side down :(
            if (viewport.IsRotated) SKMatrix.Concat(ref matrix, matrix, SKMatrix.MakeRotationDegrees((float)-viewport.Rotation));
            SKMatrix.Concat(ref matrix, matrix, SKMatrix.MakeTranslation((float)-viewport.Center.X, (float)-viewport.Center.Y));
            return matrix;
        }
    }
}
