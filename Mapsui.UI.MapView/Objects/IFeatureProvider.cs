using Mapsui.Nts;
using Mapsui.Styles;

namespace Mapsui.UI.Objects;

public interface IFeatureProvider
{
    GeometryFeature? Feature { get; }
}
