using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderService : IRenderService
{
    public RenderService(int vectorCacheCapacity = 10000)
    {
        DrawableImageCache = new DrawableImageCache();
        TileCache = new TileCache();
        ImageSourceCache = new ImageSourceCache();
        PaintCache = new PaintCache(this, vectorCacheCapacity);
        LabelCache = new LabelCache(PaintCache);
        VectorCache = new VectorCache(PaintCache, vectorCacheCapacity);
    }

    public DrawableImageCache DrawableImageCache { get; }
    public VectorCache VectorCache { get; }
    public PaintCache PaintCache { get; }
    public TileCache TileCache { get; }
    public LabelCache LabelCache { get; }
    public ImageSourceCache ImageSourceCache { get; }

    public void Dispose()
    {
        LabelCache.Dispose();
        DrawableImageCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
    }
}
