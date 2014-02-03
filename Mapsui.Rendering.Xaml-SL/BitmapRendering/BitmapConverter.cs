using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml.BitmapRendering
{
    public class BitmapConverter
    {
        public static MemoryStream ConvertToBitmapStream(int width, int height, Canvas canvas)
        {
            var writeableBitmap = new WriteableBitmap(width, height);
            writeableBitmap.Render(canvas, null);
            writeableBitmap.Invalidate();
            return ConvertToBitmapStream(writeableBitmap);
        }

        public static MemoryStream ConvertToBitmapStream(WriteableBitmap bitmap)
        {
            var stream = new MemoryStream();

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            var ei = new EditableImage(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int pixel = bitmap.Pixels[(i * width) + j];
                    ei.SetPixel(j, i,
                                (byte)((pixel >> 16) & 0xFF),
                                (byte)((pixel >> 8) & 0xFF),
                                (byte)(pixel & 0xFF),
                                (byte)((pixel >> 24) & 0xFF)
                        );
                }
            }
            Stream png = ei.GetStream();
            var len = (int)png.Length;
            var bytes = new byte[len];
            png.Read(bytes, 0, len);
            stream.Write(bytes, 0, len);

            return stream;
        }
    }
}
