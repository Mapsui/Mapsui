using System.IO;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class BitmapHelper
    {
        public static GRContext GRContext;

        public static BitmapInfo LoadBitmap(object bitmapStream)
        {
            // todo: Our BitmapRegistry stores not only bitmaps. Perhaps we should store a class in it
            // which has all information. So we should have a SymbolImageRegistry in which we store a
            // SymbolImage. Which holds the type, data and other parameters.
            if (bitmapStream is Stream stream)
            {
                byte[] buffer = new byte[4];

                stream.Position = 0;
                stream.Read(buffer, 0, 4);
                stream.Position = 0;

                if (System.Text.Encoding.UTF8.GetString(buffer, 0, 4).ToLower().Equals("<svg"))
                {
                    var svg = new SkiaSharp.Extended.Svg.SKSvg();
                    svg.Load(stream);

                    return new BitmapInfo {Svg = svg};
                }

                var image = SKImage.FromEncodedData(SKData.CreateCopy(stream.ToBytes()));

                SKImage textureBackedImage;

                // Create a texture backend image from bitmap
                var info = new SKImageInfo(image.Width, image.Height);
                using (var surface = SKSurface.Create(GRContext, false, info))
                {
                    surface.Canvas.DrawImage(image, 0, 0);
                    textureBackedImage = surface.Snapshot();
                }

                return new BitmapInfo {Bitmap = textureBackedImage };
            }

            if (bitmapStream is Sprite sprite)
            {
                return new BitmapInfo {Sprite = sprite};
            }

            return null;
        }     
    }
}