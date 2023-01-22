using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public class MapInfoRecord
{
    public MapInfoRecord(IFeature feature, IStyle style, ILayer layer)
    {
        Layer = layer;
        Feature = feature;
        Style = style;
    }

    public IFeature Feature { get; set; }
    public IStyle Style { get; set; }
    public ILayer Layer { get; set; }
}
