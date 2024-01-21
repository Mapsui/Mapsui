using System;
using Mapsui.Extensions;
using Mapsui.Styles;
using SkiaSharp;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderCache : IRenderCache<SKPath, SKPaint>
{
    private IVectorCache<SKPath, SKPaint> _vectorCache;

    public RenderCache(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        _vectorCache = new VectorCache(SymbolCache, capacity);
        TileCache = new TileCache();
        LabelCache = new LabelCache();
    }

    public ILabelCache LabelCache { get; set; }

    public ISymbolCache SymbolCache { get; set; }

    public IVectorCache<SKPath,SKPaint> VectorCache
    {
        get => _vectorCache;
        set => _vectorCache = value ?? throw new NullReferenceException();
    }

    public ITileCache TileCache { get; set; }
    
    IVectorCache IRenderCache.VectorCache
    {
        get => VectorCache;
        set => VectorCache = (IVectorCache<SKPath, SKPaint>)value;
    }

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

    public CacheTracker<SKPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, SKPaint> toPaint)
        where TParam : notnull
    {
        return VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public CacheTracker<SKPaint> GetOrCreatePaint<TParam>(TParam param, Func<TParam, ISymbolCache, SKPaint> toPaint)
        where TParam : notnull
    {
        return VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public CacheTracker<SKPath> GetOrCreatePath<TParam>(TParam param, Func<TParam, SKPath> toSkRect)
        where TParam : notnull
    {
        return VectorCache.GetOrCreatePath(param, toSkRect);
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
        VectorCache.Dispose();
        TileCache.Dispose();
    }
}
