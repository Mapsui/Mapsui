using Mapsui.Rendering.Caching;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Caching;

public sealed class TileCacheEntry(SKObject skObject) : ITileCacheEntry
{
    private readonly SKObject _skObject = skObject;

    public object Data => _skObject;
    public long IterationUsed { get; set; }
}
