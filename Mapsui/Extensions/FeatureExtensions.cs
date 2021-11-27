using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Extensions
{
    public static class FeatureExtensions
    {
        public static T Copy<T>(this T original) where T : IFeature
        {
            return (T)Activator.CreateInstance(typeof(T), original);
        }

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