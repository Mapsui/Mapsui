using System.Collections.Generic;
using Mapsui.Styles;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public class SymbolCache : Dictionary<int, BitmapImage>, ISymbolCache
    {
        public ImageSource GetOrCreate(int bitmapId)
        {
            if (ContainsKey(bitmapId)) return this[bitmapId];

            var stream = BitmapRegistry.Instance.Get(bitmapId);
            byte[] buffer = new byte[4];

            stream.Position = 0;
            stream.Read(buffer, 0, 4);

            if (System.Text.Encoding.UTF8.GetString(buffer).ToLower().Equals("<svg"))
            {
                stream.Position = 0;
                var image = Svg2Xaml.SvgReader.Load(stream);
                // Freeze the DrawingImage for performance benefits.
                image.Freeze();
                return image;
            }
            else
                return this[bitmapId] = stream.ToBitmapImage();
        }
        
        public Size GetSize(int bitmapId)
        {
            var brush = GetOrCreate(bitmapId);

            return new Size(brush.Width, brush.Height);
        }
    }
}