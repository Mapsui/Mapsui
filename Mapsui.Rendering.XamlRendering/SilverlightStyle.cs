using Mapsui.Styles;
#if !NETFX_CORE
using Media = System.Windows.Media;
using WinPoint = System.Windows.Point;
using WinColor = System.Windows.Media.Color;
#else
using Media = Windows.UI.Xaml.Media;
using WinPoint = Windows.Foundation.Point;
using WinColor = Windows.UI.Color;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    static class SilverlightStyle
    {
        public static WinColor Convert(this Color color)
        {
            return WinColor.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
        }

        //public static new System.Windows.Media.Pen Convert(this Pen pen)
        //{
        //    return new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(pen.Color.Convert()), pen.Width);
        //}

        public static Media.Brush Convert(this Brush brush)
        {
            return new Media.SolidColorBrush(brush.Color.Convert());
        }

        //public static System.Drawing.Bitmap Convert(this Bitmap bitmap)
        //{
        //    return new System.Drawing.Bitmap(bitmap.data);
        //}

        public static WinPoint Convert(this Offset offset)
        {
            return new WinPoint(offset.X, offset.Y);
        }
    }
}