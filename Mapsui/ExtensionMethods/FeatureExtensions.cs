using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

// ReSharper disable CheckNamespace
namespace Mapsui.Providers
{
    public static class FeatureExtensions
    {
        public static IFeature Copy(this IFeature original)
        {
            return new Feature(original) {Geometry = original.Geometry.Copy()};
        }

        // This should be renamed to DeepCopy. Would have saved me two hours.
        public static IEnumerable<IFeature> Copy(this IEnumerable<IFeature> original)
        {
            return original.Select(feature => feature.Copy());
        }
    }
}