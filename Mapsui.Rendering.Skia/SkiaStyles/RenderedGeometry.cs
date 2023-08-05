using System;
using Mapsui.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

internal class RenderedGeometry
{
    private Viewport? _viewport;
    private SKPath? _path;
    private LruCache<Viewport, SKPath>? _pathCache;

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
            if (_path != null)
            {
                _pathCache ??= new(10);
                var path = _pathCache[viewport];
                _pathCache[viewport] = _path; // store old path in cache
                if (path != null)             // path was in cache
                    return _path = path;
            }
                
            return _path = func();
        }

        return Path;
    }
}
