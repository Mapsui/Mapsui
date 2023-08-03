using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

internal class RenderedGeometry
{
    private Viewport? _viewport;

    public SKPaint Paint { get; set; }
    public SKPaint FillPaint { get; set; }
    public SKPath? Path { get; set; }

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
