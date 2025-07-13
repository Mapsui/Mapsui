using Mapsui.Rendering.Caching;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Caching;

public sealed class TileCacheEntry(SKObject skObject) : ITileCacheEntry
{
    private readonly SKObject _skObject = skObject;

    public object Object => _skObject;
    public long IterationUsed { get; set; }
}
