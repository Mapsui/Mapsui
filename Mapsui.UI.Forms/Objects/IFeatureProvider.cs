using Mapsui.GeometryLayer;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        GeometryFeature? Feature { get; }
    }
}
