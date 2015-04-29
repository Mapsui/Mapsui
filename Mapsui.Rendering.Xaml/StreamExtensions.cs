using System.IO;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public static class StreamExtensions
    {
        public static BitmapImage CreateBitmapImage(this Stream imageData)
        {
            var bitmapImage = new BitmapImage();
#if SILVERLIGHT
            imageData.Position = 0;
            bitmapImage.SetSource(imageData);
#elif NETFX_CORE
            imageData.Position = 0;
            var memoryStream = new System.IO.MemoryStream();
            imageData.CopyTo(memoryStream);
            bitmapImage.SetSource(ByteArrayToRandomAccessStream(memoryStream.ToArray()).Result);
#else
            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
#endif
            return bitmapImage;
        }
    }
}
