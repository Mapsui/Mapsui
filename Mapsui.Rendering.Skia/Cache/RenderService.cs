using System;
using Mapsui.Styles;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    private IVectorCache _vectorCache;

    public RenderService(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        _vectorCache = new VectorCache(SymbolCache, capacity);
        TileCache = new TileCache();
        LabelCache = new LabelCache();
        BitmapRegistry = new Styles.BitmapRegistry(Styles.BitmapRegistry.Instance);
    }

    public ILabelCache LabelCache { get; set; }

    public ISymbolCache SymbolCache { get; set; }

    public IVectorCache VectorCache
    {
        get => _vectorCache;
        set => _vectorCache = value ?? throw new NullReferenceException();
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

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        return VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public CacheTracker<TPaint> GetOrCreatePaint<TParam, TPaint>(TParam param, Func<TParam, ISymbolCache, TPaint> toPaint)
        where TParam : notnull
        where TPaint : class
    {
        return VectorCache.GetOrCreatePaint(param, toPaint);
    }

    public CacheTracker<TPath> GetOrCreatePath<TParam, TPath>(TParam param, Func<TParam, TPath> toSkRect)
        where TParam : notnull
        where TPath : class
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

    public int Register(object bitmapData, string? key = null)
    {
        return BitmapRegistry.Register(bitmapData, key);
    }

    public object? Unregister(int id)
    {
        return BitmapRegistry.Unregister(id);
    }

    public object Get(int id)
    {
        return BitmapRegistry.Get(id);
    }

    public bool Set(int id, object bitmapData)
    {
        return BitmapRegistry.Set(id, bitmapData);
    }

    public bool TryGetBitmapId(string key, out int bitmapId)
    {
        return BitmapRegistry.TryGetBitmapId(key, out bitmapId);
    }

    public int NextBitmapId()
    {
        return BitmapRegistry.NextBitmapId();
    }

    public IBitmapRegistry BitmapRegistry { get; }
}
