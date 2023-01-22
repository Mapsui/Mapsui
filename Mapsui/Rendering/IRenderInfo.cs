using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Rendering;

public interface IRenderInfo
{
    MapInfo? GetMapInfo(double screenX, double screenY, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
}
