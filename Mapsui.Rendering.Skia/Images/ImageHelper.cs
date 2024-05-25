using System.IO;
using Mapsui.Extensions;

namespace Mapsui.Rendering.Skia.Images;

internal static class ImageHelper
{
    public static IDrawableImage? LoadBitmap(Stream stream)
    {
        if (stream.IsSvg())
            return new SvgImage(stream);
        return new BitmapImage(stream);
    }
}
