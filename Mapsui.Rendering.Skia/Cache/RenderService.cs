using System;
using Mapsui.Styles;

#pragma warning disable IDISP008 // Don't assign member with injected and created disposables

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    private IVectorCache _vectorCache;

    public RenderService(int capacity = 10000)
    {
        TileCache = new TileCache();
        LabelCache = new LabelCache();
        BitmapRegistry = new BitmapRegistry(Styles.BitmapRegistry.Instance);
        SymbolCache = new SymbolCache(BitmapRegistry);
        _vectorCache = new VectorCache(SymbolCache, capacity);

    }

    public ILabelCache LabelCache { get; set; }

    public ISymbolCache SymbolCache { get; set; }

    public IVectorCache VectorCache
    {
        get => _vectorCache;
        set => _vectorCache = value ?? throw new NullReferenceException();
    }

    public ITileCache TileCache { get; set; }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
        BitmapRegistry.Dispose();
    }

    public IBitmapRegistry BitmapRegistry { get; set; }
}
