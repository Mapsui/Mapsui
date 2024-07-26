using System;
using Mapsui.Cache;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class VectorCache(PaintCache paintCache, int capacity) : IDisposable
{
    private readonly LruCache<object, ICacheHolder> _pathParamCache = new(Math.Min(capacity, 1));

    public PaintCache PaintCache => paintCache;

    public bool Enabled { get; set; } = true;

    public CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toPath)
        where TParam : notnull
        where TPath : class
    {
        if (Enabled == false)
            return new CacheTracker<TPath>(toPath(param));

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
    }
}
