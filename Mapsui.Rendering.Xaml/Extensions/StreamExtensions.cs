using System.IO;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public static class StreamExtensions
    {
        public static BitmapImage CreateBitmapImage(this Stream imageData)
        {
            var bitmapImage = new BitmapImage();

            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }  
}