using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public static class BitmapImageExtensions
    {
        public static ImageBrush ToTiledImageBrush(this BitmapImage bitmapImage)
        {
            return new ImageBrush(bitmapImage)
            {
                Viewbox = new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight),
                Viewport = new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight),
                ViewportUnits = BrushMappingMode.Absolute,
                ViewboxUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
        }

        public static Brush ToImageBrush(this ImageSource bitmapImage)
        {
            if (bitmapImage is DrawingImage image)
                return new DrawingBrush {Drawing = image.Drawing};
            else
                return new ImageBrush { ImageSource = bitmapImage };
        }
    }
}
