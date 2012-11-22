using System;
using System.Linq;

namespace SharpMap.Styles
{
    public class Pen
    {
        public Pen()
        {
            Width = 1;
        }

        public double Width { get; set; }
        public Color Color { get; set; }

        #region Equals operator

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
            if (Width != pen.Width)
            {
                return false;
            }

            if (!Color.Equals(pen.Color))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Color.GetHashCode();
        }

        public static bool operator ==(Pen pen1, Pen pen2)
        {
            return Equals(pen1, pen2);
        }

        public static bool operator !=(Pen pen1, Pen pen2)
        {
            return !Equals(pen1, pen2);
        }

        #endregion
    }
}
