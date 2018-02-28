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

        public double Width { get; set; } = 1;
        public Color Color { get; set; }

        public PenStyle PenStyle { get; set; } = PenStyle.Solid;

        public PenStrokeCap PenStrokeCap { get; set; } = PenStrokeCap.Butt;

        public PenStrokeJoin PenStrokeJoin { get; set; } = PenStrokeJoin.Miter;

        public float PenStrokeMiterLimit { get; set; } = 10f; // Default on Wpf, on Skia, it is 4f

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

            if (PenStrokeCap != pen.PenStrokeCap) return false;

            if (PenStrokeJoin != pen.PenStrokeJoin) return false;

            if (PenStrokeMiterLimit != pen.PenStrokeMiterLimit) return false;

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
