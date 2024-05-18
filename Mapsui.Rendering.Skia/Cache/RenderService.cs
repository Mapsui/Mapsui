using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    public RenderService(int vectorCacheCapacity = 10000)
    {
        SymbolCache = new SymbolCache();
        TileCache = new TileCache();
        LabelCache = new LabelCache();
        ImagePathCache = ImagePathCache.Instance;
        VectorCache = new VectorCache(this, vectorCacheCapacity);
        SpriteCache = new SpriteCache();
    }

    public ISymbolCache SymbolCache { get; }
    public IVectorCache VectorCache { get; }
    public ITileCache TileCache { get; }
    public ILabelCache LabelCache { get; }
    public ImagePathCache ImagePathCache { get; }
    public ISpriteCache SpriteCache { get; }

    public void Dispose()
    {
        LabelCache.Dispose();
        SymbolCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
        ImagePathCache.Dispose();
        SpriteCache.Dispose();
    }
}
