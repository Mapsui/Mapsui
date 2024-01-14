using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(ISymbolCache symbolCache, int capacity) : IVectorCache
{
    private readonly LruCache<object, object> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, object> _pathParamCache = new(Math.Min(capacity, 1));
    private readonly LruCache<(MRect? Rect, double Resolution, object Geometry, float lineWidth), object> _pathCache = new(Math.Max(capacity, 1));

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, T> toPaint) where T : class?
    {
        if (!_paintCache.TryGetValue(param!, out var paint))
        {
            paint = toPaint(param);
            _paintCache[param!] = paint!;
        }

        return (T?)paint;
    }

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, ISymbolCache, T> toPaint) where T : class?
    {
        if (!_paintCache.TryGetValue(param!, out var paint))
        {
            paint = toPaint(param, symbolCache);
            _paintCache[param!] = paint!;
        }

        return (T?)paint;
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        if (!_pathParamCache.TryGetValue(param!, out var rect))
        {
            rect = toSkRect(param);
            _pathParamCache[param!] = rect!;
        }

        return (T)rect!;
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _pathCache.Clear();
        _paintCache.Clear();
    }
}
