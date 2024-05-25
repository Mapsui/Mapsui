using SkiaSharp;
using Svg.Skia;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering.Skia.Extensions;

public static class StringExtensions
{
    /// <summary> Load Svg from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull(nameof(str))]
    public static SKSvg? LoadSvg(this string? str)
    {
        if (str == null)
        {
            return null;
        }

        var svg = new SKSvg();
        svg.FromSvg(str);
        return svg;
    }

    /// <summary> Load Svg Picture from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull(nameof(str))]
    public static SKPicture? LoadSvgPicture(this string? str)
    {
        return str.LoadSvg()?.Picture;
    }
}
