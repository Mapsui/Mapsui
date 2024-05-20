using System;
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
    public static BitmapInfo? LoadBitmap(object? bitmapStream, bool ownsBitmap = true) // LoadBitmap should always be Stream
    {
        // todo: Our BitmapRegistry stores not only bitmaps. Perhaps we should store a class in it
        // which has all information. So we should have a SymbolImageRegistry in which we store a
        // SymbolImage. Which holds the type, data and other parameters.

        if (bitmapStream is SKImage skBitmap)
        {
            return new BitmapInfo(ownsBitmap) { Bitmap = skBitmap, };
        }

        if (bitmapStream is SKPicture skPicture)
        {
            return new BitmapInfo(ownsBitmap) { Picture = skPicture };
        }

        if (bitmapStream is string str)
        {
            if (str.ToLower().Contains("<svg"))
            {
                return new BitmapInfo { Svg = new SvgWithStream(str.LoadSvg(), new MemoryStream()) }; //!!! str should not be supported
            }
        }

        if (bitmapStream is byte[] data)
        {
            if (data.IsXml())
            {
                using var tempStream = new MemoryStream(data);
                if (tempStream.IsSvg())
                {
                    return new BitmapInfo { Svg = new SvgWithStream(tempStream.LoadSvg(), tempStream) }; // byte[] should not be supported
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
                return new BitmapInfo { Svg = new SvgWithStream(stream.LoadSvg(), stream) };
            }

            using var skData = SKData.CreateCopy(stream.ToBytes());
            var image = SKImage.FromEncodedData(skData);
            return new BitmapInfo { Bitmap = image };
        }

        if (bitmapStream is BitmapRegion)
        {
            throw new Exception("A bitmap stream should never be a Sprite. The Sprite class has a different purpose after Mapsui 5.0.0-beta.1.");
        }

        return null;
    }

    public static bool InvalidBitmapInfo([NotNullWhen(false)] BitmapInfo? bitmapInfo)
    {
        return bitmapInfo == null || bitmapInfo.IsDisposed || (bitmapInfo.Bitmap == null && (bitmapInfo.Picture == null || bitmapInfo.Picture.IsDisposed()));
    }
}
