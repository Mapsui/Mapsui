using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.UI;
using System.Collections.Generic;

namespace Mapsui.Rendering
{
    public interface IRenderInfo
    {
        MapInfo GetMapInfo(double screenX, double screenY, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
        MapInfo GetMapInfo(Point screenPosition, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);

    }
}
