using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IVectorCache : IDisposable
{
}

public interface IVectorCache<TPath, TPaint> : IVectorCache
{
    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<TPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull;

    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<TPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, ISymbolCache, TPaint> toPaint)
        where TParam : notnull;

    CacheTracker<TPath> GetOrCreatePath<TParam>(TParam param, Func<TParam, TPath> toPath)
        where TParam : notnull;
}
