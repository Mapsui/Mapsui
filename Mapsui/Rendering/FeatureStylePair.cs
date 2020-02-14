using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public class FeatureStylePair
    {
        public FeatureStylePair(IFeature feature, IStyle style)
        {
            Feature = feature;
            Style = style;
        }

        IFeature Feature { get; set; }
        IStyle Style { get; set; }
    }
}
