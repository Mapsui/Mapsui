using System;
using Mapsui.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(ISymbolCache symbolCache, int capacity) : IVectorCache
{
    private readonly LruCache<object, CacheHolder<object>> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, CacheHolder<object>> _pathParamCache = new(Math.Min(capacity, 1));

    public CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, T> toPaint) 
    {
        var holder = _paintCache.GetOrCreateValue(param!, f =>
        {
            var paint = toPaint(f);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });
        
        if (!holder.TryGet<T>(out var result))
        {
            result = new CacheTracker<T>(holder, toPaint(param));
        };

        return result.Value;
    }

    public CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, ISymbolCache, T> toPaint)
    {
        var holder = _paintCache.GetOrCreateValue(param!, f =>
        {
            var paint =toPaint(f, symbolCache);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });
        
        if (!holder.TryGet<T>(out var result))
        {
            result = new CacheTracker<T>(holder, toPaint(param, symbolCache));
        };

        return result.Value;
    }

    public CacheTracker<T> GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toPath)
    {
        var holder = _paintCache.GetOrCreateValue(param!, f =>
        {
            var paint =toPath(f);
            return paint != null ? new CacheHolder<object>(paint) : null;
        });
        
        if (!holder.TryGet<T>(out var result))
        {
            result = new CacheTracker<T>(holder, toPath(param));
        };

        return result.Value;
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _paintCache.Clear();
    }
}
