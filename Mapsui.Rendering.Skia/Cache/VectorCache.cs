using System;
using Mapsui.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(ISymbolCache symbolCache, int capacity) : IVectorCache<SKPath, SKPaint>
{
    private readonly LruCache<object, CacheHolder<SKPaint>> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, CacheHolder<SKPath>> _pathParamCache = new(Math.Min(capacity, 1));

    public CacheTracker<SKPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, SKPaint> toPaint)
        where TParam : notnull
    {
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f);
            return new CacheHolder<SKPaint>(paint);
        });

        return holder?.Get<SKPaint>() ?? new CacheTracker<SKPaint>(toPaint(param));
    }

    public CacheTracker<SKPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, ISymbolCache, SKPaint> toPaint)
        where TParam : notnull
    {
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f, symbolCache);
            return new CacheHolder<SKPaint>(paint);
        });
        
        return holder?.Get<SKPaint>() ?? new CacheTracker<SKPaint>(toPaint(param, symbolCache));
    }

    public CacheTracker<SKPath> GetOrCreatePath<TParam>(TParam param, Func<TParam, SKPath> toPath)
        where TParam : notnull
    {
        var holder = _pathParamCache.GetOrCreateValue(param, f =>
        {
            var path = toPath(f);
            return new CacheHolder<SKPath>(path);
        });
        
        return holder?.Get<SKPath>() ?? new CacheTracker<SKPath>(toPath(param));
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _paintCache.Clear();
    }
}
