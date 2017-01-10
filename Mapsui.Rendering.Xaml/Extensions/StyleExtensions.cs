using Mapsui.Styles;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;
using Media = System.Windows.Media;
using WinPoint = System.Windows.Point;
using WinColor = System.Windows.Media.Color;

namespace Mapsui.Rendering.Xaml
{
    static class StyleExtensions
    {
        public static Media.DoubleCollection ToXaml(this PenStyle penStyle)
        {
            return StyleConverter.MapsuiPentoXaml(penStyle);
        }

        public static WinColor ToXaml(this Color color)
        {
            if (color == null) return WinColor.FromArgb(0, 255, 255, 255);
            return WinColor.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
        }
        
        public static Media.Brush ToXaml(this Brush brush, BrushCache brushCache = null)
        {
            return StyleConverter.MapsuiBrushToXaml(brush, brushCache);
        }

        public static WinPoint ToXaml(this Offset offset)
        {
            return new WinPoint(offset.X, offset.Y);
        }
    }
}