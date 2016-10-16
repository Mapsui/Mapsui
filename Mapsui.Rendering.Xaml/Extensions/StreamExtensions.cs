using System.IO;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System;
#else
using System.Windows.Media.Imaging;
#endif

namespace Mapsui.Rendering.Xaml.Extensions
{
    public static class StreamExtensions
    {
        public static BitmapImage CreateBitmapImage(this Stream imageData)
        {
            var bitmapImage = new BitmapImage();
#if NETFX_CORE
            imageData.Position = 0;
            var memoryStream = new MemoryStream();
            imageData.CopyTo(memoryStream);
            bitmapImage.SetSource(ToRandomAccessStream(memoryStream).Result);
#else
            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
#endif
            return bitmapImage;
        }


#if NETFX_CORE
        public static async Task<IRandomAccessStream> ToRandomAccessStream(this MemoryStream memoryStream)
        {
            var tile = memoryStream.ToArray();
            var stream = new InMemoryRandomAccessStream();
            var dataWriter = new DataWriter(stream);
            dataWriter.WriteBytes(tile);
            await dataWriter.StoreAsync();
            stream.Seek(0);
            return stream;
        }
#endif
    }  
}
