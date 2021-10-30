using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.Providers
{
    public interface IGeometryFeature : IFeature
    {
        public IGeometry? Geometry { get; set; }
    }
}
