// ReSharper disable CheckNamespace
namespace Mapsui.Geometries
{
    public static class GeometryExtensions
    {
        public static IGeometry Copy(this IGeometry original)
        {
            return Geometry.GeomFromWKB(original.AsBinary());
        }
    }
}