using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IVectorCache : IDisposable
{
    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, T> toPaint);

    [return: NotNullIfNotNull(nameof(param))]
    CacheTracker<T> GetOrCreatePaint<T, TParam>(TParam param, Func<TParam, ISymbolCache, T> toPaint);

    CacheTracker<T> GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toPath);
}
