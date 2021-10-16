using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapsui.Providers;

namespace Mapsui.Extensions
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

        public static string ToDisplayText(this IFeature feature)
        {
            var result = new StringBuilder();
            foreach (var field in feature.Fields)
                result.Append($"{field}:{feature[field]}");
            return result.ToString();
        }

        public static string ToDisplayText(this IEnumerable<KeyValuePair<string, IEnumerable<IFeature>>> featureInfos)
        {
            var result = new StringBuilder();

            foreach (var layer in featureInfos)
            {
                result.Append(layer.Key);
                result.Append(Environment.NewLine);
                foreach (var feature in layer.Value)
                {
                    result.Append(feature.ToDisplayText());
                }
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }
    }
}