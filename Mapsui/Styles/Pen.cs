// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles
{
    public class Pen
    {
        public Pen() {}

        public Pen(Color color, double width = 1)
        {
            Color = color;
            Width = width;
        }

        /// <summary>
        /// Width of line
        /// </summary>
        public double Width { get; set; } = 1;

        /// <summary>
        /// Color of line
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Style of the line (solid/dashed), which is drawn
        /// </summary>
        public PenStyle PenStyle { get; set; } = PenStyle.Solid;

        /// <summary>
        /// Array for drawing user defined dashes. Should be even and values are 
        /// multiplied by line width before drawing.
        /// </summary>
        public float[] DashArray { get; set; } = null;

        /// <summary>
        /// Defines the end of a line
        /// </summary>
        public PenStrokeCap PenStrokeCap { get; set; } = PenStrokeCap.Butt;

        /// <summary>
        /// Defines how line parts are join together
        /// </summary>
        public StrokeJoin StrokeJoin { get; set; } = StrokeJoin.Miter;

        /// <summary>
        /// Defines up to which width of line StrokeJoin is used
        /// </summary>
        public float StrokeMiterLimit { get; set; } = 10f; // Default on Wpf, on Skia, it is 4f

        public override bool Equals(object obj)
        {
            if (!(obj is Pen))
            {
                return false;
            }
            return Equals((Pen)obj);
        }

        public bool Equals(Pen pen)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Width != pen.Width) return false;
            
            //if one or the other is null then they are not equal, but not when they are both null
            if ((Color == null) ^ (pen.Color == null)) return false;

            if (Color != null && !Color.Equals(pen.Color)) return false;

            if (PenStyle != pen.PenStyle) return false;

            if (DashArray != pen.DashArray) return false;

            if (PenStrokeCap != pen.PenStrokeCap) return false;

            if (StrokeJoin != pen.StrokeJoin) return false;

            if (StrokeMiterLimit != pen.StrokeMiterLimit) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ (Color == null ? 0 : Color.GetHashCode());
        }

        public static bool operator ==(Pen pen1, Pen pen2)
        {
            return Equals(pen1, pen2);
        }

        public static bool operator !=(Pen pen1, Pen pen2)
        {
            return !Equals(pen1, pen2);
        }
    }
}
