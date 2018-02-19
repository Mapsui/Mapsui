using System.Collections.Generic;
using Mapsui.Styles;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public class SymbolCache : Dictionary<int, BitmapImage>, ISymbolCache
    {
        public BitmapImage GetOrCreate(int bitmapId)
        {
            if (ContainsKey(bitmapId)) return this[bitmapId];

            var stream = BitmapRegistry.Instance.Get(bitmapId);
            byte[] buffer = new byte[4];

            stream.Position = 0;
            stream.Read(buffer, 0, 4);

            if (System.Text.Encoding.UTF8.GetString(buffer).ToLower().Equals("<svg"))
            {
                // TODO: Convert Svg to Bitmap with Skia?
                stream.Position = 0;
                return null;
            }
            else
                return this[bitmapId] = stream.ToBitmapImage();
        }
        
        public Size GetSize(int bitmapId)
        {
            var brush = GetOrCreate(bitmapId);

            // TODO: Remove this, if Svg is implemented
            if (brush == null)
                return new Size();

            return new Size(brush.Width, brush.Height);
        }
    }
}