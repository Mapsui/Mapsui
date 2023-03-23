using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Rendering;

public interface IRenderInfo
{
    MapInfo? GetMapInfo(double screenX, double screenY, ViewportState viewport, IEnumerable<ILayer> layers, int margin = 0);
}
