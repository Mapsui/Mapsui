using Svg.Skia;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Mapsui.Rendering.Skia.Extensions;
public static class ByteExtensions
{

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
