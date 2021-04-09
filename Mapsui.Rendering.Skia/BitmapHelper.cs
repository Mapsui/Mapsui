using System.IO;
using Mapsui.Extensions;
using Mapsui.Styles;
using SkiaSharp;
using Svg.Skia;


namespace Mapsui.Rendering.Skia
{
    public static class BitmapHelper
    {
        public static BitmapInfo LoadBitmap(object bitmapStream)
        {
            // todo: Our BitmapRegistry stores not only bitmaps. Perhaps we should store a class in it
            // which has all information. So we should have a SymbolImageRegistry in which we store a
            // SymbolImage. Which holds the type, data and other parameters.

            if (bitmapStream is string str)
            {
                if (str.ToLower().Contains("<svg"))
                {
                    var svg = new SKSvg();
                    svg.FromSvg(str);

                    return new BitmapInfo { Svg = svg };
                }
            }

            if (bitmapStream is Stream stream)
            {
                if (stream.IsSvg())
                {
                    var svg = new SKSvg();
                    svg.Load(stream);

                    return new BitmapInfo {Svg = svg};
                }

                var image = SKImage.FromEncodedData(SKData.CreateCopy(stream.ToBytes()));
                return new BitmapInfo {Bitmap = image};
            }

            if (bitmapStream is Sprite sprite)
            {
                return new BitmapInfo {Sprite = sprite};
            }

            return null;
        }
    }
}