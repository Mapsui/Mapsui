using System.Diagnostics.CodeAnalysis;
using System.IO;
using SkiaSharp;
using Svg.Skia;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Rendering.Skia.Images;

public static class SvgHelper
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

    /// <summary> Load Svg from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull(nameof(str))]
    public static SKSvg LoadSvg(this Stream str)
    {
        var svg = new SKSvg();
        svg.Load(str);
        return svg;
    }

    /// <summary> Load Svg from byte array</summary>
    /// <param name="bytes">svg data</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull(nameof(bytes))]
    public static SKSvg LoadSvg(this byte[] bytes)
    {
        var svg = new SKSvg();
        svg.Load(new MemoryStream(bytes));
        return svg;
    }
}
