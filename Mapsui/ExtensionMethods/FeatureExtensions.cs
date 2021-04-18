using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

// ReSharper disable CheckNamespace
namespace Mapsui.Providers
{
    public static class FeatureExtensions
    {
        public static IGeometryFeature Copy(this IGeometryFeature original)
        {
            return new Feature(original) {Geometry = original.Geometry.Copy()};
        }

        public static IEnumerable<IGeometryFeature> Copy(this IEnumerable<IGeometryFeature> original)
        {
            return original.Select(f => f.Copy()).ToList();
        }
    }
}