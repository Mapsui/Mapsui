using SkiaSharp;
using System.Drawing;

namespace Mapsui.Rendering.Skia.Extensions;

public static class SKColorExtensions
{
    public static Color ToMapsui(this SKColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }
}
