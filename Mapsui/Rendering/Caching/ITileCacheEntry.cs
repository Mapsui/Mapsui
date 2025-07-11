using System;

namespace Mapsui.Rendering.Skia.Cache;

public interface ITileCacheEntry : IDisposable
{
    public long IterationUsed { get; set; }

    public object Object { get; }
}
