using Mapsui.Extensions;

namespace Mapsui.Rendering.Skia.Images;

internal static class ImageHelper
{
    public static IDrawableImage ToDrawableImage(byte[] bytes)
    {
        if (bytes.IsSvg())
            return new SvgImage(bytes);
        return new BitmapImage(bytes);
    }
}
