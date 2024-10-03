using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(IRenderService renderService, int capacity) : IDisposable
{
    private readonly LruCache<object, ICacheHolder> _cache = new(Math.Min(capacity, 1));

    public bool Enabled { get; set; } = true;

    public CacheTracker<TPaint> GetOrCreate<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param));

#pragma warning disable IDISP001
        var holder = _cache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f);
            return new CacheHolder<TPaint>(paint);
        });
#pragma warning restore IDISP001

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param));
    }

    public CacheTracker<TPaint> GetOrCreate<TParam, TPaint>(TParam param, Func<TParam, IRenderService, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param, renderService));

#pragma warning disable IDISP001
        var holder = _cache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f, renderService);
            return new CacheHolder<TPaint>(paint);
        });

#pragma warning restore IDISP001
        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param, renderService));
    }

    public void Dispose()
    {
        _cache.Clear();
    }
}
