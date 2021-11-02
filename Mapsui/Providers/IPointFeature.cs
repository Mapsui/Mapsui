using Mapsui.Layers;

namespace Mapsui.Providers
{
    public interface IPointFeature : IFeature
    {
        public MPoint Point { get; }
    }
}
