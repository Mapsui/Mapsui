using System.Collections.Generic;

namespace Mapsui.Layers;

internal class RenderLayer : BaseLayer
{
    private readonly IEnumerable<IFeature> _features;

    public RenderLayer(ILayer layer, IEnumerable<IFeature> features)
    {
        Style = layer.Style;
        _features = features;
        Attribution = layer.Attribution;
        Opacity = layer.Opacity;
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _features;
    }
}
