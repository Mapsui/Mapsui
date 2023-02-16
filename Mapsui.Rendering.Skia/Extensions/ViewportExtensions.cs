﻿using Mapsui.Extensions;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class ViewportExtensions
{
    public static SKMatrix ToSKMatrix(this IViewport viewport)
    {
        var mapCenterX = (float)viewport.State.Width * 0.5f;
        var mapCenterY = (float)viewport.State.Height * 0.5f;
        var invertedResolution = 1f / (float)viewport.State.Resolution;

        var matrix = SKMatrix.CreateScale(invertedResolution, invertedResolution, mapCenterX, mapCenterY);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(1, -1, 0, -mapCenterY)); // As a consequence images will be up side down :(
        if (viewport.State.IsRotated()) matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)-viewport.State.Rotation));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)-viewport.State.CenterX, (float)-viewport.State.CenterY));
        return matrix;
    }

    /// <summary> Converts the Extent of the Viewport to a SKRect </summary>
    /// <param name="viewport">viewport</param>
    /// <returns>SkRect</returns>
    public static SKRect ToSkiaRect(this IReadOnlyViewport viewport)
    {
        return viewport.WorldToScreen(viewport.State.GetExtent()).ToSkia();
    }
}
