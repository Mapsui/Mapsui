using System.Text;
using Mapsui.Providers;

namespace Mapsui.Samples.Avalonia.ExtensionMethods
{
    static class FeatureExtensions
    {
        public static string ToDisplayText(this IFeature feature)
        {
            var result = new StringBuilder();
            foreach (var field in feature.Fields)
                result.Append($"{field}:{feature[field]}");
            return result.ToString();
        }
    }
}