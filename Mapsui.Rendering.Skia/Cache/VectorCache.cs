using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(ISymbolCache symbolCache, int capacity) : IVectorCache
{
    private readonly LruCache<object, object> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, object> _pathParamCache = new(Math.Min(capacity, 1));
    private readonly LruCache<(MRect? Rect, double Resolution, object Geometry, float lineWidth), object> _pathCache = new(Math.Max(capacity, 1));

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, T> toPaint)
        where T : class?
    {
        return _paintCache.GetOrCreateValue(param!, toPaint);
    }

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, ISymbolCache, T> toPaint)
        where T : class?
    {
        return _paintCache.GetOrCreateValue(param!, f => toPaint(f, symbolCache));
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        return _pathParamCache.GetOrCreateValue(param!, toSkRect);
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _pathCache.Clear();
        _paintCache.Clear();
    }
}
