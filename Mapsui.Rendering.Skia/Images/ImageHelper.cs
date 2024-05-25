using Mapsui.Extensions;

namespace Mapsui.Rendering.Skia.Images;

internal static class ImageHelper
{
    public static IDrawableImage LoadBitmap(byte[] bytes)
    {
        if (bytes.IsSvg())
            return new SvgImage(bytes);
        return new BitmapImage(bytes);
    }
}
