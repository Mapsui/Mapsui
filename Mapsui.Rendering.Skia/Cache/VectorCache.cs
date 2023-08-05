using System;
using System.Collections.Concurrent;
using Mapsui.Cache;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public class VectorCache : IVectorCache
{
    private readonly ConcurrentDictionary<(object? Pen, float Opacity), object> _paintCache = new();
    private readonly ConcurrentDictionary<(Brush? Brush, float Opacity, double rotation), object> _fillCache = new();
    private readonly LruCache<object, object> _pathParamCache;
    private readonly ISymbolCache _symbolCache;

    public VectorCache(ISymbolCache symbolCache, int capacity)
    {
        _pathParamCache = new(Math.Min(capacity, 1));
        _symbolCache = symbolCache;
    }

    public T? GetOrCreatePaint<T, TPen>(TPen? pen, float opacity, Func<TPen?, float, T> toPaint) where T : class?
    {
        var key = (pen, opacity);
        if (!_paintCache.TryGetValue(key, out var paint))
        {
            paint = toPaint(pen, opacity);
            _paintCache[key] = paint!;
        }

        return (T?)paint;
    }

    public T? GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint) where T : class?
    {
        var key = (pen: brush, opacity, rotation);
        if (!_fillCache.TryGetValue(key, out var paint))
        {
            paint = toPaint(brush, opacity, rotation, _symbolCache);
            _fillCache[key] = paint!;
        }

        return (T?)paint;
    }

    public T GetOrCreatePath<T, TParam>(TParam viewport, Func<TParam, T> toSkRect)
    {
        if (!_pathParamCache.TryGetValue(viewport!, out var rect))
        {
            rect = toSkRect(viewport);
            _pathParamCache[viewport!] = rect!;
        }

        return (T)rect!;
    }
}
