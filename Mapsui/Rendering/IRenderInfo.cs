using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Rendering;

public interface IRenderInfo
{
    MapInfoBase? GetMapInfo(double screenX, double screenY, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0);
}
