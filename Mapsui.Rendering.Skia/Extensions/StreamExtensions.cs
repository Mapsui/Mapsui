using Svg.Skia;
using System.IO;

namespace Mapsui.Rendering.Skia.Extensions;

public static class StreamExtensions
{
    /// <summary> Load Svg from String </summary>
    /// <param name="str">string</param>
    /// <returns>loaded svg image</returns>
    public static SKSvg LoadSvg(this Stream str)
    {
        var svg = new SKSvg();
#pragma warning disable IDISP004
        svg.Load(str);
#pragma warning restore IDISP004
        return svg;
    }
}
