using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(IRenderService symbolCache, int capacity) : IVectorCache
{
    private readonly LruCache<object, ICacheHolder> _paintCache = new(Math.Min(capacity, 1));
    private readonly LruCache<object, ICacheHolder> _pathParamCache = new(Math.Min(capacity, 1));

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f);
            return new CacheHolder<TPaint>(paint);
        });

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param));
    }

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, IRenderService, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f, symbolCache);
            return new CacheHolder<TPaint>(paint);
        });

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param, symbolCache));
    }

    public CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toPath)
        where TParam : notnull
        where TPath : class
    {
        var holder = _pathParamCache.GetOrCreateValue(param, f =>
        {
            var path = toPath(f);
            return new CacheHolder<TPath>(path);
        });

        return holder?.Get<TPath>() ?? new CacheTracker<TPath>(toPath(param));
    }

    public void Dispose()
    {
        _pathParamCache.Clear();
        _paintCache.Clear();
    }
}
