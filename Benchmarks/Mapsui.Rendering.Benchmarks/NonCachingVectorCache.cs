using Mapsui.Styles;

namespace Mapsui.Rendering.Benchmarks;

public class NonCachingVectorCache : IVectorCache
{
    private readonly ISymbolCache _symbolCache;

    public NonCachingVectorCache(ISymbolCache symbolCache)
    {
        _symbolCache = symbolCache;
    }

    public T? GetOrCreatePaint<T, TPen>(TPen? pen, float opacity, Func<TPen?, float, T> toPaint) where T : class?
    {
        return toPaint(pen, opacity);
    }

    public T? GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint) where T : class?
    {
        return toPaint(brush, opacity, rotation, _symbolCache);
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        return toSkRect(param);
    }

    public TPath GetOrCreatePath<TPath, TGeometry>(Viewport viewport, TGeometry geometry, float lineWidth, Func<TGeometry, Viewport, float, TPath> toPath, Func<TGeometry, TGeometry>? copy) where TPath : class where TGeometry : class
    {
        return toPath(geometry, viewport, lineWidth);
    }
}
