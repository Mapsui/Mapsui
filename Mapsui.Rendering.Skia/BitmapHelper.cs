using System.Diagnostics.CodeAnalysis;
using System.IO;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using Mapsui.Utilities;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public static class BitmapHelper
{
    public static BitmapInfo? LoadBitmap(object? bitmapStream)
    {
        // todo: Our BitmapRegistry stores not only bitmaps. Perhaps we should store a class in it
        // which has all information. So we should have a SymbolImageRegistry in which we store a
        // SymbolImage. Which holds the type, data and other parameters.

        if (bitmapStream is SKImage skBitmap)
        {
            return new BitmapInfo { Bitmap = skBitmap, };
        }

        if (bitmapStream is SKPicture skPicture)
        {
            return new BitmapInfo { Picture = skPicture };
        }

        if (bitmapStream is string str)
        {
            if (str.ToLower().Contains("<svg"))
            {
                return new BitmapInfo { Svg = str.LoadSvg() };
            }
        }

        if (bitmapStream is byte[] data)
        {
            if (data.IsXml())
            {
                using var tempStream = new MemoryStream(data);
                if (tempStream.IsSvg())
                {
                    return new BitmapInfo { Svg = tempStream.LoadSvg() };
                }
            }
            else if (data.IsSkp())
            {
                return new BitmapInfo { Picture = SKPicture.Deserialize(data) };
            }

            using var skData = SKData.CreateCopy(data);
            var image = SKImage.FromEncodedData(skData);
            return new BitmapInfo { Bitmap = image };
        }

        if (bitmapStream is Stream stream)
        {
            if (stream.IsSvg())
            {
                return new BitmapInfo { Svg = stream.LoadSvg() };
            }

            using var skData = SKData.CreateCopy(stream.ToBytes());
            var image = SKImage.FromEncodedData(skData);
            return new BitmapInfo { Bitmap = image };
        }

        if (bitmapStream is Sprite sprite)
        {
            return new BitmapInfo { Sprite = sprite };
        }

        return null;
    }

    public static bool InvalidBitmapInfo([NotNullWhen(false)] BitmapInfo? bitmapInfo)
    {
        return bitmapInfo == null || (bitmapInfo.Bitmap == null && (bitmapInfo.Picture == null || bitmapInfo.Picture.IsDisposed()));
    }
}
