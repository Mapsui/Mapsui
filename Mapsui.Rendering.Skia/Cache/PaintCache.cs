using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class PaintCache(IRenderService renderService, int capacity) : IDisposable
{
    private readonly LruCache<object, ICacheHolder> _paintCache = new(Math.Min(capacity, 1));

    public bool Enabled { get; set; } = true;

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param));

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
        if (Enabled == false)
            return new CacheTracker<TPaint>(toPaint(param, renderService));

        var holder = _paintCache.GetOrCreateValue(param, f =>
        {
            var paint = toPaint(f, renderService);
            return new CacheHolder<TPaint>(paint);
        });

        return holder?.Get<TPaint>() ?? new CacheTracker<TPaint>(toPaint(param, renderService));
    }
    public void Dispose()
    {
        _paintCache.Clear();
    }
}
