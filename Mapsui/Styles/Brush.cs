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
            if (Color != brush.Color) return false;
            return true;
        }

        public override int GetHashCode()
        {            
            return (Color == null) ? 0 : Color.GetHashCode();
        }

        #endregion

    }
}
