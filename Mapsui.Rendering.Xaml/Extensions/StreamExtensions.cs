using System.IO;
using System.Windows;
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

        public static System.Windows.Media.ImageBrush ToTiledImageBrush(this Stream stream)
        {
            var bitmap = stream.ToBitmapImage();

            var imageBrush = new System.Windows.Media.ImageBrush(bitmap)
            {
                Viewbox = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                Viewport = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                ViewportUnits = System.Windows.Media.BrushMappingMode.Absolute,
                ViewboxUnits = System.Windows.Media.BrushMappingMode.Absolute,
                TileMode = System.Windows.Media.TileMode.Tile
            };

            return imageBrush;
        }
    }  
}