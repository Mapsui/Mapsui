using System;
using System.Linq;

namespace SharpMap.Styles
{
    public class Brush
    {
        //TODO: add other brush attributes 
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
            if (!Color.Equals(brush.Color)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
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
