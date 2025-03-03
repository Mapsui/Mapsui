namespace Mapsui.Styles;

/// <summary>
/// Offset relative to the size of the item to which it applies. The unit of measure
/// is the width or height of an image. A relative offset of X = 0, and Y = 0 will be centered.
/// An offset of (0.5, 0.5) the symbol will be moved half the width of the image to the right and half the 
/// height of the image to the top. So the bottom left point of the image will be on
/// the location.
/// </summary>
public class RelativeOffset
{
    public RelativeOffset() { }

    public RelativeOffset(double x, double y)
    {
        X = x;
        Y = y;
    }

    public RelativeOffset(RelativeOffset offset) : this(offset.X, offset.Y) { }

    public RelativeOffset(MPoint point) : this(point.X, point.Y) { }

    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;

    /// <summary>
    /// Calculate the real offset respecting width and height
    /// </summary>
    /// <param name="width">Width of the symbol</param>
    /// <param name="height">Height of the symbol</param>
    /// <returns>Calculated offset</returns>
    public Offset GetAbsoluteOffset(double width, double height)
    {
        return new Offset(width * X, height * Y);
    }
}
