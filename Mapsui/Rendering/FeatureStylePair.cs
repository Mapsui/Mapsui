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

      public IFeature Feature { get; set; }
        public IStyle Style { get; set; }
    }
}
