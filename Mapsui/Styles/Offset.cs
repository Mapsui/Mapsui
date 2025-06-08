namespace Mapsui.Styles;

public record Offset(double X, double Y)
{
    /// <summary>
    /// Offset of images from the center of the image.
    /// </summary>
    public Offset() : this(0, 0) { }
    public Offset(MPoint point) : this(point.X, point.Y) { }

    public MPoint ToPoint() => new(X, Y);

    public Offset Combine(Offset offset) => new(X + offset.X, Y + offset.Y);
}
