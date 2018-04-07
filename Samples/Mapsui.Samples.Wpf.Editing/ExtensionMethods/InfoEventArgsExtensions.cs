using Mapsui.UI;

namespace Mapsui.Samples.Wpf.Editing
{
    static class InfoEventArgsExtensions
    {
        public static string ToDisplayText(this MapInfo args)
        {
            return $"World Position={args.WorldPosition.X:F0},{args.WorldPosition.Y:F0}\n" +
                   $"Feature={args.Feature.ToDisplayText()}";
        }
    }
}
