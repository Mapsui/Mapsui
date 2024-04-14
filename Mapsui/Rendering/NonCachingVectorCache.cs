using System;

namespace Mapsui.Rendering;

public sealed class NonCachingVectorCache(IRenderService symbolCache) : IVectorCache
{
    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        return new CacheTracker<TPaint>(toPaint(param));
    }

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, IRenderService, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        return new CacheTracker<TPaint>(toPaint(param, symbolCache));
    }

    public CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toSkRect)
        where TParam : notnull
        where TPath : class
    {
        return new CacheTracker<TPath>(toSkRect(param));
    }

    public void Dispose()
    {
    }
}
