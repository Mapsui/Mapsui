using Mapsui.Extensions;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class ViewportExtensions
{
    public static SKMatrix ToSKMatrix(this ViewportState viewportState)
    {
        var mapCenterX = (float)viewportState.Width * 0.5f;
        var mapCenterY = (float)viewportState.Height * 0.5f;
        var invertedResolution = 1f / (float)viewportState.Resolution;

        var matrix = SKMatrix.CreateScale(invertedResolution, invertedResolution, mapCenterX, mapCenterY);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(1, -1, 0, -mapCenterY)); // As a consequence images will be up side down :(
        if (viewportState.IsRotated()) matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)-viewportState.Rotation));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)-viewportState.CenterX, (float)-viewportState.CenterY));
        return matrix;
    }

    public static SKMatrix ToSKMatrix(this ViewportState viewportState, SKMatrix matrix)
    {
        return SKMatrix.Concat(viewportState.ToSKMatrix(), matrix);
    }

    /// <summary> Converts the Extent of the Viewport to a SKRect </summary>
    /// <param name="viewport">viewport</param>
    /// <returns>SkRect</returns>
    public static SKRect ToSkiaRect(this ViewportState viewport)
    {
        return viewport.WorldToScreen(viewport.ToExtent()).ToSkia();
    }
}
