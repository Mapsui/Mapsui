using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Mapsui.Rendering.Xaml.BitmapRendering
{
    public class BitmapConverter
    {
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
    }
}
