using System;
using System.IO;
#if !NETFX_CORE
using System.Windows.Media.Imaging;
#else
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
#endif

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
