using Mapsui.Styles;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;
using Media = System.Windows.Media;
using WinPoint = System.Windows.Point;
using WinColor = System.Windows.Media.Color;
using System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    static class StyleExtensions
    {
        public static DoubleCollection ToXaml(this PenStyle penStyle, float[] dashArray = null)
        {
            return StyleConverter.MapsuiPentoXaml(penStyle, dashArray);
        }

        public static PenLineCap ToXaml(this PenStrokeCap penStrokeCap)
        {
            return StyleConverter.MapsuiStrokeCaptoPenLineCap(penStrokeCap);
        }

        public static PenLineJoin ToXaml(this StrokeJoin penStrokeJoin)
        {
            return StyleConverter.MapsuiStrokeJointoPenLineJoin(penStrokeJoin);
        }

        public static WinColor ToXaml(this Color color)
        {
            if (color == null) return WinColor.FromArgb(0, 255, 255, 255);
            return WinColor.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
        }
        
        public static Media.Brush ToXaml(this Brush brush, SymbolCache symbolCache = null, float rotate = 0)
        {
            return StyleConverter.MapsuiBrushToXaml(brush, symbolCache, rotate);
        }

        public static WinPoint ToXaml(this Offset offset)
        {
            return new WinPoint(offset.X, offset.Y);
        }
    }
}