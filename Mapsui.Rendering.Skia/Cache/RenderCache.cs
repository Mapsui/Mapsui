using System;
using Mapsui.Extensions;
using Mapsui.Styles;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderCache : IRenderCache
{
    private IVectorCache? _vectorCache;

    public RenderCache(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        VectorCache = new VectorCache(SymbolCache, capacity);
        TileCache = new TileCache();
        LabelCache = new LabelCache();
    }

    public ILabelCache LabelCache { get; set; }

    public ISymbolCache SymbolCache { get; set; }

    public IVectorCache? VectorCache
    {
        get => _vectorCache;
        set => _vectorCache = value;
    }

    public ITileCache TileCache { get; set; }

    public Size? GetSize(int bitmapId)
    {
        return SymbolCache.GetSize(bitmapId);
    }

    public IBitmapInfo GetOrCreate(int bitmapID)
    {
        return SymbolCache.GetOrCreate(bitmapID);
    }

    public T GetOrCreateTypeface<T>(Font font, Func<Font, T> createTypeFace) where T : class
    {
        return LabelCache.GetOrCreateTypeface(font, createTypeFace);
    }

    public T GetOrCreateLabel<T>(string? text, LabelStyle style, float opacity, Func<LabelStyle, string?, float, ILabelCache, T> createLabelAsBitmap) where T : IBitmapInfo
    {
        return LabelCache.GetOrCreateLabel(text, style, opacity, createLabelAsBitmap);
    }

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, T> toPaint) where T : class?
    {
        return VectorCache == null ? toPaint(param) : VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public T? GetOrCreatePaint<TParam, T>(TParam param, Func<TParam, ISymbolCache, T> toPaint) where T : class?
    {
        return VectorCache == null ? toPaint(param, SymbolCache) : VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public T GetOrCreatePath<T, TParam>(TParam param, Func<TParam, T> toSkRect)
    {
        return VectorCache == null ? toSkRect(param) : VectorCache.GetOrCreatePath(param, toSkRect);
    }

    public IBitmapInfo? GetOrCreate(MRaster raster, long currentIteration)
    {
        return TileCache.GetOrCreate(raster, currentIteration);
    }

    public void UpdateCache(long iteration)
    {
        TileCache.UpdateCache(iteration);
    }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        DisposableExtension.DisposeAndNullify(ref _vectorCache);
        TileCache.Dispose();
    }
}
