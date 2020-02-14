using Mapsui.Layers;
using System.Collections.Generic;

namespace Mapsui.Rendering
{
    public interface IRenderInfo
    {
        List<FeatureStylePair> GetMapInfo(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers);
    }
}
