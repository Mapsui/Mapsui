namespace Mapsui.Nts.Providers;

public interface IFeatureProvider
{
    GeometryFeature? Feature { get; }
}
