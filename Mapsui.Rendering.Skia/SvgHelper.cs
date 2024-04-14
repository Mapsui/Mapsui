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

    public static int LoadSvgId(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource, IBitmapRegistry bitmapRegistry)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
        if (!bitmapRegistry.TryGetBitmapId(fullName, out var bitmapId))
        {
            Stream? tempQualifier = assembly.GetManifestResourceStream(fullName);
            var result = tempQualifier.LoadSvg()?.Picture;
            if (result != null)
            {
                bitmapId = bitmapRegistry.Register(result, fullName);
                return bitmapId;
            }
        }

        return bitmapId;
    }

    public static Uri LoadSvgPath(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        return EmbeddedResourceLoader.GetResourceUri(typeInAssemblyOfEmbeddedResource, relativePathToEmbeddedResource);
    }
}
