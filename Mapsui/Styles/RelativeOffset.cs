namespace Mapsui.Styles;

/// <summary>
/// Relative offset of an image to the center of the source. The unit of measure
/// is the width or height of an image. So in case of an an offset of (0.5, 0.5) 
/// the symbol will be moved half the width of the image to the right and half the 
/// height of the image to the top. So the bottom left point of the image will be on
/// the location.
/// </summary>
public class RelativeOffset : Offset
{
    public RelativeOffset() { }

    public RelativeOffset(double x, double y) : base(x, y) { }

    public RelativeOffset(Offset offset) : base(offset) { }

    public RelativeOffset(MPoint point) : base(point) { }

    /// <summary>
    /// Calculate the real offset respecting width and height
    /// </summary>
    /// <param name="width">Width of the symbol</param>
    /// <param name="height">Height of the symbol</param>
    /// <returns>Calculated offset</returns>
    public override Offset CalcOffset(double width, double height)
    {
        return new Offset(width * X, height * Y);
    }
}
