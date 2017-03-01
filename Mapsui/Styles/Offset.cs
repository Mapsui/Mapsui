namespace Mapsui.Styles
{
    public class Offset
    {
        public Offset() {}

        public Offset(Offset offset)
        {
            X = offset.X;
            Y = offset.Y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Offset))
                return false;
            return Equals((Offset) obj);
        }

        public bool Equals(Offset offset)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != offset.X) return false;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
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