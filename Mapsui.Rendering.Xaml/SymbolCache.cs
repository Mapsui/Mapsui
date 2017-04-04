using System.Collections.Generic;
using Mapsui.Styles;
using XamlMedia = System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public class SymbolCache : Dictionary<int, XamlMedia.ImageBrush>, ISymbolCache
    {
        // Try to get an imagebrush from cache by given BitmapRegistry id, if not exist
        // create a new brush and return it.
        public XamlMedia.ImageBrush GetTiledImageBrush(int bitmapId)
        {
            if (ContainsKey(bitmapId)) return this[bitmapId];

            return this[bitmapId] = BitmapRegistry.Instance.Get(bitmapId).ToTiledImageBrush();
        }

        public XamlMedia.ImageBrush GetImageBrush(int bitmapId)
        {
            if (ContainsKey(bitmapId)) return this[bitmapId];
            
            return (this[bitmapId] = CreateImageBrush(bitmapId));
        }

        private static XamlMedia.ImageBrush CreateImageBrush(int bitmapId)
        {
            var bitmapImage = BitmapRegistry.Instance.Get(bitmapId).ToBitmapImage();
            var brush = new XamlMedia.ImageBrush {ImageSource = bitmapImage};
            return brush;
        }

        public double GetWidth(int bitmapId)
        {
            return GetImageBrush(bitmapId).ImageSource.Width;
        }

        public double GetHeight(int bitmapId)
        {
            // This creates a regular symbol and not the tile one. This could be incorrect
            return GetImageBrush(bitmapId).ImageSource.Height;
        }

        public Size GetSize(int bitmapId)
        {
            var brush = GetImageBrush(bitmapId);
            return new Size(brush.ImageSource.Width, brush.ImageSource.Height);
        }
    }
}
