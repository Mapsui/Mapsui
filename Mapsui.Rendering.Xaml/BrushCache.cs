using System;
using System.Collections.Generic;
using Mapsui.Styles;
#if NETFX_CORE
using XamlMedia = Windows.UI.Xaml.Media;
#else
using XamlMedia = System.Windows.Media;
#endif

namespace Mapsui.Rendering.Xaml
{
    public class BrushCache : Dictionary<int, XamlMedia.ImageBrush>
    {
        // Try to get an imagebrush from cache by given BitmapRegistry id, if not exist
        // create a new brush and return it.
        public XamlMedia.ImageBrush GetImageBrush(int bmpId, Func<System.IO.Stream, XamlMedia.ImageBrush> createBrushFunc = null)
        {
            if (ContainsKey(bmpId))
                return this[bmpId];

            XamlMedia.ImageBrush brush;
            var data = BitmapRegistry.Instance.Get(bmpId);

            if (createBrushFunc != null)
            {
                brush = createBrushFunc(data);
            }
            else
            {
                var bitmapImage = data.CreateBitmapImage();
                brush = new XamlMedia.ImageBrush { ImageSource = bitmapImage };
            }

            this[bmpId] = brush;

            return brush;
        }
    }
}
