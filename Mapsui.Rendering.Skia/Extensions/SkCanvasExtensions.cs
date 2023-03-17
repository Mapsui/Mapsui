using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class SKCanvasExtensions
{
    public static IDisposable SetMatrix(this SKCanvas skCanvas, ViewportState viewportState)
    {
        var matrix = skCanvas.TotalMatrix;
        var newMatrix = viewportState.ToSKMatrix(matrix);
        skCanvas.SetMatrix(newMatrix);

        return new MatrixKeeper(matrix, skCanvas);
    }
}
