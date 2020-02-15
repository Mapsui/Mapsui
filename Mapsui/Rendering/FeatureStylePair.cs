using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public class MapInfoRecord
    {
        public MapInfoRecord(ILayer layer, IFeature feature, IStyle style)
        {
            Layer = layer;
            Feature = feature;
            Style = style;
        }

        public ILayer Layer { get; set; }
        public IFeature Feature { get; set; }
        public IStyle Style { get; set; }
    }
}
