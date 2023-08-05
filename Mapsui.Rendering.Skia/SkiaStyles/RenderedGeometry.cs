using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

internal class RenderedGeometry
{
    private Viewport? _viewport;
    private SKPath? _path;

    public SKPaint? LinePaint { get; init; }
    public SKPaint? FillPaint { get; init; }
    public SKPath? Path 
    {
        get { return _path; }
        init { _path = value; }
    }

    public SKPath GetOrCreatePath(Viewport viewport, Func<SKPath> func)
    {
        if (Path == null || _viewport != viewport)
        {
            _viewport = viewport;
            return _path = func();
        }

        return Path;
    }
}
