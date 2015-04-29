using System;
using System.Linq;

namespace Mapsui.Styles
{
    //created this class as port of GDI's PointF, but I am not at all sure if we really need it. 
    //I prefer to use an offsetX and offsetY. PDD.
    public class Offset
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Offset(){}

        public Offset(Offset offset)
        {
            X = offset.X;
            Y = offset.Y;
        }

        
        public override bool Equals(object obj)
        {
            if (!(obj is Offset))
            {
                return false;
            }
            return Equals((Offset)obj);
        }

        public bool Equals(Offset offset)
        {
            if (X != offset.X) return false;
            if (Y != offset.Y) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(Offset offset1, Offset offset2)
        {
            return Equals(offset1, offset2);
        }

        public static bool operator !=(Offset offset1, Offset offset2)
        {
            return !Equals(offset1, offset2);
        }

            }
}
