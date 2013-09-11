using System;
using System.Linq;

namespace Mapsui.Styles
{
    public class Brush
    {
        public Brush() {}

        public Brush(Color color)
        {
            Color = color;
        }

        public Brush(Brush brush)
        {
            Color = brush.Color;
        }

        public Color Color { get; set; }

        #region Equals operator

        public override bool Equals(object obj)
        {
            if (!(obj is Brush))
            {
                return false;
            }
            return Equals((Brush)obj);
        }

        public bool Equals(Brush brush)
        {
            if ((Color == null) ^ (brush.Color == null))
            {
                return false;
            }

            if (Color != brush.Color)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {            
            return (Color == null) ? 0 : Color.GetHashCode();
        }

        public static bool operator ==(Brush brush1, Brush brush2)
        {
            return Equals(brush1, brush2);
        }

        public static bool operator !=(Brush brush1, Brush brush2)
        {
            return !Equals(brush1, brush2);
        }

        #endregion

    }
}
