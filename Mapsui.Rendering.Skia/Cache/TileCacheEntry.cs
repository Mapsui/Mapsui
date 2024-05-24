using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tiling;

public class TileCacheEntry(SKObject skObject)
{
    public SKObject SKObject { get; } = skObject;
    public long IterationUsed { get; set; }
}
