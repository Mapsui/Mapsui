using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Utilities;

public static class SvgHelper
{
    /// <summary> Load Svg from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull("str")]
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
    [return: NotNullIfNotNull("str")]
    public static SKPicture? LoadSvgPicture(this string? str)
    {
        return str.LoadSvg()?.Picture;
    }

    /// <summary> Load Svg from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull("str")]
    public static SKSvg? LoadSvg(this Stream? str)
    {
        if (str == null)
        {
            return null;
        }

        var svg = new SKSvg();
        svg.Load(str);
        return svg;
    }

    /// <summary> Load Svg Picture from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    [return: NotNullIfNotNull("str")]
    public static SKPicture? LoadSvgPicture(this Stream? str)
    {
        return str.LoadSvg()?.Picture;
    }

    public static int LoadSvgId(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
        if (!BitmapRegistry.Instance.TryGetBitmapId(fullName, out var bitmapId))
        {
            var result = assembly.GetManifestResourceStream(fullName).LoadSvgPicture();
            if (result != null)
            {
                bitmapId = BitmapRegistry.Instance.Register(result, fullName);
                return bitmapId;
            }
        }

        return bitmapId;
    }
}
