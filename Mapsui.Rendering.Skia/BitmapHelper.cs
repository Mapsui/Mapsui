using System.IO;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Tiling;
using Mapsui.Utilities;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public static class BitmapHelper
{
    public static IRenderedTile? LoadTileBitmap(byte[] data)
    {
        if (data.IsSKPicture())
        {
            return new PictureTile(SKPicture.Deserialize(data));
        }

        using var skData = SKData.CreateCopy(data);
        var image = SKImage.FromEncodedData(skData);
        return new ImageTile(image);
    }

    public static ImageTile? LoadBitmap(byte[] data)
    {
        using var skData = SKData.CreateCopy(data);
        var image = SKImage.FromEncodedData(skData);
        return new ImageTile(image);
    }

    public static BitmapInfo? LoadBitmap(Stream stream)
    {
        if (stream.IsSvg())
        {
            return new BitmapInfo { Svg = new SvgWithStream(stream.LoadSvg(), stream) };
        }

        using var skData = SKData.CreateCopy(stream.ToBytes());
        var image = SKImage.FromEncodedData(skData);
        return new BitmapInfo { Bitmap = image };

    }

    public static bool InvalidBitmapInfo(BitmapInfo bitmapInfo)
    {
        return bitmapInfo.IsDisposed || (bitmapInfo.Bitmap == null && (bitmapInfo.Picture == null || bitmapInfo.Picture.IsDisposed()));
    }
}
