using System.Windows.Media.Imaging;
using System.IO;

namespace Mapsui.Rendering.Xaml
{
    public static class Utilities
    {
        public static MemoryStream ConverToBitmapStream(WriteableBitmap bitmap)
        {
            //Thanks Eiji for posting on msdn:
            //http://forums.silverlight.net/forums/p/114691/446894.aspx
            //A nice simple static method which seems a more direct route than
            //ConverToBitmapStream. There is still an exception in my scenario.
            //I would like to figure out what is wrong exactly so I can use this 
            //method instead of the clunky one above.

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            var ms = new MemoryStream();

            #region BMP File Header(14 bytes)
            //the magic number(2 bytes):BM
            ms.WriteByte(0x42);
            ms.WriteByte(0x4D);

            //the size of the BMP file in bytes(4 bytes)
            long len = bitmap.Pixels.Length * 4 + 0x36;

            ms.WriteByte((byte)len);
            ms.WriteByte((byte)(len >> 8));
            ms.WriteByte((byte)(len >> 16));
            ms.WriteByte((byte)(len >> 24));

            //reserved(2 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //reserved(2 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the offset(4 bytes)
            ms.WriteByte(0x36);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            #endregion

            #region Bitmap Information(40 bytes:Windows V3)
            //the size of this header(4 bytes)
            ms.WriteByte(0x28);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the bitmap width in pixels(4 bytes)
            ms.WriteByte((byte)width);
            ms.WriteByte((byte)(width >> 8));
            ms.WriteByte((byte)(width >> 16));
            ms.WriteByte((byte)(width >> 24));

            //the bitmap height in pixels(4 bytes)
            ms.WriteByte((byte)height);
            ms.WriteByte((byte)(height >> 8));
            ms.WriteByte((byte)(height >> 16));
            ms.WriteByte((byte)(height >> 24));

            //the number of color planes(2 bytes)
            ms.WriteByte(0x01);
            ms.WriteByte(0x00);

            //the number of bits per pixel(2 bytes)
            ms.WriteByte(0x20);
            ms.WriteByte(0x00);

            //the compression method(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the image size(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the horizontal resolution of the image(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the vertical resolution of the image(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the number of colors in the color palette(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);

            //the number of important colors(4 bytes)
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            ms.WriteByte(0x00);
            #endregion

            #region Bitmap data
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = bitmap.Pixels[width * y + x];

                    ms.WriteByte((byte)(pixel & 0xff)); //B
                    ms.WriteByte((byte)((pixel >> 8) & 0xff)); //G
                    ms.WriteByte((byte)((pixel >> 0x10) & 0xff)); //R
                    ms.WriteByte(0x00); //reserved
                }
            }
            #endregion

            return new MemoryStream(ms.GetBuffer());
        }
    }
}
