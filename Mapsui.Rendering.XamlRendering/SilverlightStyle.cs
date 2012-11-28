using Mapsui.Styles;

namespace SilverlightRendering
{
    static class SilverlightStyle
    {
        public static System.Windows.Media.Color Convert(this Color color)
        {
            return System.Windows.Media.Color.FromArgb((byte)color.A, (byte)color.R, (byte)color.G, (byte)color.B);
        }

        //public static new System.Windows.Media.Pen Convert(this Pen pen)
        //{
        //    return new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(pen.Color.Convert()), pen.Width);
        //}

        public static System.Windows.Media.Brush Convert(this Brush brush)
        {
            return new System.Windows.Media.SolidColorBrush(brush.Color.Convert());
        }

        //public static System.Drawing.Bitmap Convert(this Bitmap bitmap)
        //{
        //    return new System.Drawing.Bitmap(bitmap.data);
        //}

        public static System.Windows.Point Convert(this Offset offset)
        {
            return new System.Windows.Point(offset.X, offset.Y);
        }
    }
}