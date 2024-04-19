using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    public RenderService(int capacity = 10000)
    {
        TileCache = new TileCache();
        LabelCache = new LabelCache();
        BitmapRegistry = new BitmapRegistry(Styles.BitmapRegistry.Instance);
        SymbolCache = new SymbolCache(BitmapRegistry);
        VectorCache = new VectorCache(this, capacity);
    }

    public ISymbolCache SymbolCache { get; }
    public IVectorCache VectorCache { get; }
    public ITileCache TileCache { get; }
    public ILabelCache LabelCache { get; }
    public IBitmapRegistry BitmapRegistry { get; }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
        BitmapRegistry.Dispose();
    }
}
