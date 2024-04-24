using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    public RenderService(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        VectorCache = new VectorCache(SymbolCache, capacity);
        TileCache = new TileCache();
        LabelCache = new LabelCache();
    }

    public ISymbolCache SymbolCache { get; }
    public IVectorCache VectorCache { get; }
    public ITileCache TileCache { get; }
    public ILabelCache LabelCache { get; }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
    }
}
