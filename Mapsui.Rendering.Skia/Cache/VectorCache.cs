using System;
using Mapsui.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(ISymbolCache symbolCache, int capacity) : IVectorCache
{
    private readonly LruCache<object, CacheHolder<object>> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, CacheHolder<object>> _pathParamCache = new(Math.Min(capacity, 1));

    public CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, T> toPaint)
        where TParam : notnull
    {
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });

        return holder?.Get<T>() ?? new CacheTracker<T>(toPaint(param));
    }

    public CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, ISymbolCache, T> toPaint)
        where TParam : notnull
    {
        var holder = _paintCache.GetOrCreateValue(param!, f =>
        {
            var paint =toPaint(f, symbolCache);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });
        
        return holder?.Get<T>() ?? new CacheTracker<T>(toPaint(param, symbolCache));
    }

    public CacheTracker<T> GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toPath)
        where TParam : notnull
    {
        var holder = _paintCache.GetOrCreateValue(param!, f =>
        {
            var paint =toPath(f);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });
        
        return holder?.Get<T>() ?? new CacheTracker<T>(toPath(param));
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _paintCache.Clear();
    }
}
