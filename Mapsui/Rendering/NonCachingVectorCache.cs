using System;

namespace Mapsui.Rendering;

public sealed class NonCachingVectorCache(ISymbolCache symbolCache) : IVectorCache
{
    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, T> toPaint) where T : class?
    {
        return toPaint(param);
    }

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, ISymbolCache, T> toPaint) where T : class?
    {
        return toPaint(param, symbolCache);
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        return toSkRect(param);
    }

    public void Dispose()
    {
    }
}
