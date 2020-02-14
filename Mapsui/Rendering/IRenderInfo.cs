using Mapsui.Layers;
using System.Collections.Generic;

namespace Mapsui.Rendering
{
    public interface IRenderInfo
    {
        List<UI.MapInfo> GetMapInfo(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers);
    }
}
