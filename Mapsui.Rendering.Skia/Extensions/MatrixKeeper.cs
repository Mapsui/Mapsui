using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

internal class MatrixKeeper : IDisposable
{
    private SKCanvas? _canvas;
    private readonly SKMatrix _matrix;

    public MatrixKeeper(ViewportState viewportState, SKCanvas canvas)
    {
        _matrix = viewportState.ToSKMatrix(canvas.TotalMatrix);
        _canvas = canvas;
    }

    public MatrixKeeper(SKMatrix matrix, SKCanvas canvas)
    {
        _matrix = matrix;
        _canvas = canvas;
    }

    public void Dispose()
    {
        _canvas?.SetMatrix(_matrix);
        _canvas = null;
    }
}
