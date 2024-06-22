using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class SKPictureExtensions
{
    public static Size GetSize(this SKPicture skPicture)
    {
        var bounds = skPicture.CullRect;
        return new Size(bounds.Width, bounds.Height);
    }
}
