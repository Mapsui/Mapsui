using Mapsui.Layers;
using Mapsui.UI;
using System.Collections.Generic;

namespace Mapsui.Rendering
{
    public interface IRenderInfo
    {
        MapInfo GetMapInfo(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
    }
}
