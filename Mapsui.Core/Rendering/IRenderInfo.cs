using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Rendering
{
    public interface IRenderInfo
    {
        MapInfo? GetMapInfo(double screenX, double screenY, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
        MapInfo? GetMapInfo(MPoint screenPosition, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
        Task<MapInfo?> GetMapInfoAsync(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0);
    }
}
