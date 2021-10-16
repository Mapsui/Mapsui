using Mapsui.Extensions;
using Mapsui.Providers;
using Mapsui.UI;

namespace Mapsui.Samples.Wpf.Editing
{
    static class InfoEventArgsExtensions
    {
        public static string ToDisplayText(this MapInfo mapInfo)
        {
            if (mapInfo.Feature is IGeometryFeature geometryFeature)
            {
                return $"World Position={mapInfo.WorldPosition.X:F0},{mapInfo.WorldPosition.Y:F0}\n" +
                       $"Feature={geometryFeature.ToDisplayText()}";
            }

            return $"Not a {nameof(IGeometryFeature)}";
        }
    }
}
