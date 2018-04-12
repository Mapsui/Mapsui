using System.Linq;
using System.Text;
using Mapsui.Providers;

namespace Mapsui.Samples.Wpf.Editing
{
    static class FeatureExtensions
    {
        public static string ToDisplayText(this IFeature feature)
        {
            if (feature == null) return "no feature";
            if (!feature.Fields.Any()) return "feature with no attributes";
            var result = new StringBuilder();
            foreach (var field in feature.Fields)
                result.Append($"{field}:{feature[field]}");
            return result.ToString();
        }
    }
}