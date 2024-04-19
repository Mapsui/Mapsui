using System;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering;

public interface IVectorCache : IDisposable
{
    public bool Enabled { get; set; }

    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class;

    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, ISymbolCache, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class;

    CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toPath)
        where TParam : notnull
        where TPath : class;
}
