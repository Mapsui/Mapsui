using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.Extensions
{
    public static class FeatureExtensions
    {
        public static IFeature Copy(this IFeature original)
        {
            return (IFeature)Activator.CreateInstance(original.GetType(), original);
        }

        public static IEnumerable<IFeature> Copy(this IEnumerable<IFeature> original)
        {
            return original.Select(f => f.Copy()).ToList();
        }
    }
}