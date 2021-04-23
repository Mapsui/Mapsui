using Mapsui.Providers;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        IGeometryFeature Feature { get; }
    }
}
