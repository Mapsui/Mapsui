namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderCache : IRenderService
{
    public RenderCache(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        VectorCache = new VectorCache(SymbolCache, capacity);
        TileCache = new TileCache();
        LabelCache = new LabelCache();
    }

    public ISymbolCache SymbolCache { get; }
    public IVectorCache VectorCache { get; private set; }
    public ITileCache TileCache { get; }
    public ILabelCache LabelCache { get; }

    public void DisableVectorCache()
    {
        VectorCache.Dispose();
        VectorCache = new NonCachingVectorCache(SymbolCache);
    }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
    }
}
