using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Rendering;

public interface IRenderInfo
{
    Task<MapInfo?> GetMapInfoAsync(double screenX, double screenY, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0);
}
