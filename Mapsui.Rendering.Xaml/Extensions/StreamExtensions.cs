using System.IO;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public static class StreamExtensions
    {
        public static BitmapImage ToBitmapImage(this Stream imageData)
        {
            imageData.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}