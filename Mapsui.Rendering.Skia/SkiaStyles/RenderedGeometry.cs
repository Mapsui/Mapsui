using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

internal class RenderedGeometry
{
    private Viewport? _viewport;

    public SKPaint Paint { get; init; }
    public SKPaint? FillPaint { get; init; }
    public SKPath? Path { get; private set; }

    public SKPath GetOrCreatePath(Viewport viewport, Func<SKPath> func)
    {
        if (Path == null || _viewport != viewport)
        {
            _viewport = viewport;
            return Path = func();
        }

        return Path;
    }
}
