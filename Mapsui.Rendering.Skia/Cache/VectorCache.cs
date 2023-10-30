using System;
using System.Collections.Concurrent;
using System.Reflection;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public class VectorCache : IVectorCache
{
    private readonly ConcurrentDictionary<(object? Pen, float Opacity), object> _paintCache = new();
    private readonly ConcurrentDictionary<(Brush? Brush, float Opacity, double rotation), object> _fillCache = new();
    private readonly LruCache<object, object> _pathParamCache;
    private readonly LruCache<(MRect? Rect, double Resolution, object Geometry, float lineWidth), object> _pathCache;
    private readonly ISymbolCache _symbolCache;

    public VectorCache(ISymbolCache symbolCache, int capacity)
    {
        _pathParamCache = new(Math.Min(capacity, 1));
        _pathCache = new (Math.Max(capacity, 1));
        _symbolCache = symbolCache;
    }

    public T? GetOrCreatePaint<T, TPen>(TPen? pen, float opacity, Func<TPen?, float, T> toPaint) where T : class?
    {
        var key = (pen, opacity);
        if (!_paintCache.TryGetValue(key, out var paint))
        {
            paint = toPaint(pen, opacity);
            _paintCache[key] = paint!;
        }

        return (T?)paint;
    }

    public T? GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint) where T : class?
    {
        var key = (pen: brush, opacity, rotation);
        if (!_fillCache.TryGetValue(key, out var paint))
        {
            paint = toPaint(brush, opacity, rotation, _symbolCache);
            _fillCache[key] = paint!;
        }

        return (T?)paint;
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        if (!_pathParamCache.TryGetValue(param!, out var rect))
        {
            rect = toSkRect(param);
            _pathParamCache[param!] = rect!;
        }

        return (T)rect!;
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
        var key = (viewport.ToExtent(), viewport.Rotation, feature.Id, lineWidth);
        if (!_pathCache.TryGetValue(key, out var path))
        {
            path = toPath(geometry, viewport, lineWidth);
            _pathCache[key] = path;
        }

        return (TPath)path;
    }
}
