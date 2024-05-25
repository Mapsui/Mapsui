using System.IO;
using Mapsui.Extensions;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Images;

internal static class ImageHelper
{
    public static IDrawableImage? LoadBitmap(Stream stream)
    {
        if (stream.IsSvg())
        {
            return new SvgImage(stream);
        }

        using var skData = SKData.CreateCopy(stream.ToBytes());
        var image = SKImage.FromEncodedData(skData);
        return new BitmapImage(image);
    }
}
