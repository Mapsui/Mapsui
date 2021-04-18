using Mapsui.Geometries;

namespace Mapsui.Providers
{
    public interface IGeometryFeature : IFeature
    {
        public IGeometry Geometry { get; set; }
    }
}
