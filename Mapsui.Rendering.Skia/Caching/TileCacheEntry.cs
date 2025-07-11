using Mapsui.Rendering.Skia.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Caching;

public sealed class TileCacheEntry(SKObject skObject) : ITileCacheEntry
{
    private readonly SKObject _skObject = skObject;
    private bool _disposed;

    public object Object => _skObject;
    public long IterationUsed { get; set; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
#pragma warning disable IDISP007
        _skObject.Dispose();
#pragma warning restore IDISP007
        _disposed = true;
    }
}
