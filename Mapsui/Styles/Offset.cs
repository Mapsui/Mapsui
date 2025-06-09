namespace Mapsui.Styles;

public class Offset(double x, double y)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public Offset() : this(0, 0) { }

    public Offset(Offset offset) : this(offset.X, offset.Y) { }

    public Offset(MPoint point) : this(point.X, point.Y) { }

    public MPoint ToPoint() => new(X, Y);

    public Offset Combine(Offset offset) => new(X + offset.X, Y + offset.Y);

    public override bool Equals(object? obj)
    {
        if (obj is not Offset offset)
            return false;
        return Equals(offset);
    }

    public bool Equals(Offset? offset)
    {
        if (offset == null)
            return false;

        if (X != offset.X) return false;
        if (Y != offset.Y) return false;
        return true;
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public static bool operator ==(Offset? offset1, Offset? offset2) => Equals(offset1, offset2);

    public static bool operator !=(Offset? offset1, Offset? offset2) => !Equals(offset1, offset2);
}
