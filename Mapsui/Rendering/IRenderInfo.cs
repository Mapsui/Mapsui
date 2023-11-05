using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Rendering;

public interface IRenderInfo
{
    [Obsolete("Use GetMapInfoAsync")]
#if NETSTANDARD2_0
    MapInfo? GetMapInfo(double screenX, double screenY, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0);
#else
    MapInfo? GetMapInfo(double screenX, double screenY, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0)
    {
#pragma warning disable VSTHRD002 // synchronously waiting
        return GetMapInfoAsync(screenX, screenY, viewport, layers, margin).Result;
#pragma warning restore VSTHRD002 // synchronously waiting
    }
#endif


    Task<MapInfo?> GetMapInfoAsync(double screenX, double screenY, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0);
}
