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

    public TPath GetOrCreatePath<TPath, TFeature, TGeometry>(
        Viewport viewport,
        TFeature feature,
        TGeometry geometry,
        float lineWidth, Func<TGeometry, Viewport, float, TPath> toPath)
        where TPath : class
        where TGeometry : class
        where TFeature : class, IFeature
    {
        return toPath(geometry, viewport, lineWidth);
    }
}
