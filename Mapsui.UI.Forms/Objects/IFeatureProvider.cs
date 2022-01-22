using Mapsui.GeometryLayers;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        GeometryFeature? Feature { get; }
    }
}
