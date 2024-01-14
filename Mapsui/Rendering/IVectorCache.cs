using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IVectorCache : IDisposable
{
    [return: NotNullIfNotNull(nameof(param))]
    T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, T> toPaint)
        where T : class?;

    T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, ISymbolCache, T> toPaint)
        where T : class?;

    T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect);
}
