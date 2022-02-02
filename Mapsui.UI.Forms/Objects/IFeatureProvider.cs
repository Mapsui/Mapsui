using Mapsui.Nts;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        GeometryFeature? Feature { get; }
    }
}
