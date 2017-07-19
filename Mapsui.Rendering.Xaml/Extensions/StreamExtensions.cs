using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public static class StreamExtensions
    {
        public static BitmapImage ToBitmapImage(this Stream imageData)
        {
            var bitmapImage = new BitmapImage();

            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static ImageBrush ToTiledImageBrush(this Stream stream)
        {
            var bitmap = stream.ToBitmapImage();

            return new ImageBrush(bitmap)
            {
                Viewbox = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                Viewport = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                ViewportUnits = BrushMappingMode.Absolute,
                ViewboxUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
        }

        public static ImageBrush ToImageBrush(this Stream stream)
        {
            return new ImageBrush { ImageSource = stream.ToBitmapImage() };
        }
    }
}