using Svg.Skia;
using System.IO;

namespace Mapsui.Rendering.Skia.Extensions;
public static class ByteExtensions
{

    /// <summary> Load Svg from byte array</summary>
    /// <param name="bytes">svg data</param>
    /// <returns>loaded svg image</returns>
    public static SKSvg LoadSvg(this byte[] bytes)
    {
        var svg = new SKSvg();
        using var memoryStream = new MemoryStream(bytes);
#pragma warning disable IDISP004
        svg.Load(memoryStream);
#pragma warning restore IDISP004
        return svg;
    }
}
