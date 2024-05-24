using System;

namespace Mapsui.Rendering;

public interface ITileCache : IDisposable
{
    void UpdateCache(long iteration);
}
