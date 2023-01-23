// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Styles;

public class Offset
{
    /// <summary>
    /// Offset of images from the center of the image.
    /// If IsRelative, than the offset is between -0.5 and +0.5.
    /// </summary>
    public Offset() { }

    public Offset(double x, double y, bool isRelative = false)
    {
        X = x;
        Y = y;
        IsRelative = isRelative;
    }

    public Offset(Offset offset, bool isRelative = false)
    {
        X = offset.X;
        Y = offset.Y;
        IsRelative = isRelative;
    }

    public double X { get; set; }
    public double Y { get; set; }
    public bool IsRelative { get; set; }

    public MPoint ToPoint()
    {
        return new MPoint(X, Y);
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is Offset offset))
            return false;
        return Equals(offset);
    }

    public bool Equals(Offset? offset)
    {
        if (offset == null)
            return false;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (X != offset.X) return false;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Y != offset.Y) return false;
        if (IsRelative != offset.IsRelative) return false;
        return true;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode() ^ IsRelative.GetHashCode();
    }

    public static bool operator ==(Offset? offset1, Offset? offset2)
    {
        return Equals(offset1, offset2);
    }

    public static bool operator !=(Offset? offset1, Offset? offset2)
    {
        return !Equals(offset1, offset2);
    }
}
