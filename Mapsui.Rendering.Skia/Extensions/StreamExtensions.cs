using Svg.Skia;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Mapsui.Rendering.Skia.Extensions;

public static class StreamExtensions
{
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
}
