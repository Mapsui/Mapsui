using Mapsui.Providers;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        Feature Feature { get; }
        bool IsVisible { get; }
    }
}
