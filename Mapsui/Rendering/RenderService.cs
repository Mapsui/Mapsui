using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using System;

namespace Mapsui.Rendering;

public sealed class RenderService : IDisposable
{
    public RenderService(int vectorCacheCapacity = 30000)
    {
        DrawableImageCache = new DrawableImageCache();
        TileCache = new TileCache();
        ImageSourceCache = new ImageSourceCache();
        VectorCache = new VectorCache(this, vectorCacheCapacity);
    }

    public DrawableImageCache DrawableImageCache { get; }
    public VectorCache VectorCache { get; }
    public TileCache TileCache { get; }
    public ImageSourceCache ImageSourceCache { get; }

    public void Dispose()
    {
        DrawableImageCache.Dispose();
        VectorCache.Dispose();
        TileCache.Dispose();
    }
}
