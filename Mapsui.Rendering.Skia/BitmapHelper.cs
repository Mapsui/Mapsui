using System.IO;
using System.Text;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class BitmapHelper
    {
        public static BitmapInfo LoadBitmap(object bitmapStream)
        {
            // todo: Our BitmapRegistry stores not only bitmaps. Perhaps we should store a class in it
            // which has all information. So we should have a SymbolImageRegistry in which we store a
            // SymbolImage. Which holds the type, data and other parameters.
            if (bitmapStream is Stream stream)
            {
                if (IsSvg(stream))
                {
                    var svg = new SkiaSharp.Extended.Svg.SKSvg();
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

        /// <summary>
        /// Detects if stream is svg stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns>true if is svg stream</returns>
        private static bool IsSvg(Stream stream)
        {
            byte[] buffer = new byte[5];

            stream.Position = 0;
            stream.Read(buffer, 0, 5);
            stream.Position = 0;

            if (Encoding.UTF8.GetString(buffer, 0, 4).ToLowerInvariant().Equals("<svg"))
            {
                return true;
            }

            if (Encoding.UTF8.GetString(buffer, 0, 5).ToLowerInvariant().Equals("<?xml"))
            {
                var svg = Encoding.UTF8.GetBytes("<svg");
                if (ReadOneSearch(stream, svg) >= 0)
                {
                    stream.Position = 0;
                    return true;
                };

                stream.Position = 0;
            }

            return false;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1471975/best-way-to-find-position-in-the-stream-where-given-byte-sequence-starts
        /// </summary>
        /// <param name="haystack">stream to search</param>
        /// <param name="needle">pattern to find</param>
        /// <returns>position</returns>
        private static long ReadOneSearch(Stream haystack, byte[] needle)
        {
            int b;
            long i = 0;
            while ((b = haystack.ReadByte()) != -1)
            {
                if (b == needle[i++])
                {
                    if (i == needle.Length)
                    {
                        return haystack.Position - needle.Length;
                    }
                }
                else if (b == needle[0])
                {
                    i = 1;
                }
                else
                {
                    i = 0;
                }
            }

            return -1;
}
    }
}