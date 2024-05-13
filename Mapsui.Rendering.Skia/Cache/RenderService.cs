using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    public RenderService(int capacity = 10000)
    {
        SymbolCache = new SymbolCache();
        TileCache = new TileCache();
        LabelCache = new LabelCache();
        BitmapRegistry = new RenderBitmapRegistry(Styles.BitmapRegistry.Instance, BitmapPathRegistry.Instance);
        VectorCache = new VectorCache(this, capacity);
        SpriteCache = new SpriteCache();
    }

    public ISymbolCache SymbolCache { get; }
    public IVectorCache VectorCache { get; }
    public ITileCache TileCache { get; }
    public ILabelCache LabelCache { get; }
    public IRenderBitmapRegistry BitmapRegistry { get; }
    public ISpriteCache SpriteCache { get; }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
        BitmapRegistry.Dispose();
        SpriteCache.Dispose();
    }
}
