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
            
            return this[bitmapId] = BitmapRegistry.Instance.Get(bitmapId).ToBitmapImage();
        }
        
        public Size GetSize(int bitmapId)
        {
            var brush = GetOrCreate(bitmapId);

            return new Size(brush.Width, brush.Height);
        }
    }
}