using Mapsui.GeometryLayer;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        IGeometryFeature? Feature { get; }
    }
}
