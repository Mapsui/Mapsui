namespace Mapsui.Styles;

/// <summary>
/// Offset relative to the size of the item to which it applies. The unit of measure
/// is the width or height of an image. A relative offset of X = 0, and Y = 0 will be centered.
/// An offset of (0.5, 0.5) the symbol will be moved half the width of the image to the right and half the 
/// height of the image to the top. So the bottom left point of the image will be on
/// the location.
/// </summary>
public class RelativeOffset(double x = 0, double y = 0)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;

    public RelativeOffset() : this(0, 0) { }
    public RelativeOffset(RelativeOffset offset) : this(offset.X, offset.Y) { }
    public RelativeOffset(MPoint point) : this(point.X, point.Y) { }

    public MPoint ToPoint() => new(X, Y);

    /// <summary>
    /// Calculate the real offset respecting width and height
    /// </summary>
    /// <param name="width">Width of the symbol</param>
    /// <param name="height">Height of the symbol</param>
    /// <returns>Calculated offset</returns>
    public Offset GetAbsoluteOffset(double width, double height) => new(width * X, height * Y);

    public override bool Equals(object? obj)
    {
        if (obj is not RelativeOffset offset)
            return false;
        return Equals(offset);
    }

    public bool Equals(RelativeOffset? offset)
    {
        if (offset == null)
            return false;

        if (X != offset.X) return false;
        if (Y != offset.Y) return false;
        return true;
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public static bool operator ==(RelativeOffset? offset1, RelativeOffset? offset2) => Equals(offset1, offset2);

    public static bool operator !=(RelativeOffset? offset1, RelativeOffset? offset2) => !Equals(offset1, offset2);
}
