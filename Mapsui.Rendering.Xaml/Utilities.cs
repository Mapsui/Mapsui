using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml
{
    public class Utilities
    {
#if !NETFX_CORE
#if SILVERLIGHT
        public static MemoryStream ToBitmapStream(UIElement uiElement, double width, double height)
        {
            uiElement.Arrange(new Rect(0, 0, width, height));

            var writeableBitmap = new WriteableBitmap((int)width, (int)height);
            writeableBitmap.Render(uiElement, null);
            var bitmapStream = Utilities.ConverToBitmapStream(writeableBitmap);
            return bitmapStream;
        }
#else
        public static MemoryStream ToBitmapStream(UIElement uiElement, double width, double height)
        {
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new PixelFormat());
            uiElement.Arrange(new Rect(0, 0, width, height));
            renderTargetBitmap.Render(uiElement);
            var bitmap = new PngBitmapEncoder();
            bitmap.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            var bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream);
            
            return bitmapStream;
        }
#endif
#endif
    }
}
