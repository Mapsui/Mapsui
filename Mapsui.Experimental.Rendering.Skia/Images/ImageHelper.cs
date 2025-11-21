using Mapsui.Extensions;
using Mapsui.Rendering;

namespace Mapsui.Experimental.Rendering.Skia.Images;

internal static class ImageHelper
{
    public static IDrawableImage ToDrawableImage(byte[] bytes)
    {
        if (bytes.IsSvg())
            return new SvgDrawableImage(bytes);
        return new BitmapDrawableImage(bytes);
    }
}
