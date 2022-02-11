using Mapsui.Utilities;

namespace Mapsui;

public class MPoint2
{
    public double X { get; set; }
    public double Y { get; set; }

    MRect MRect { get; }
    MPoint Copy();
    double Distance(MPoint point);
    bool Equals(MPoint? p);
    int GetHashCode();
    MPoint Offset(double offsetX, double offsetY);

    /// <summary>
    ///     Calculates a new point by rotating this point clockwise about the specified center point
    /// </summary>
    /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
    /// <param name="centerX">X coordinate of point about which to rotate</param>
    /// <param name="centerY">Y coordinate of point about which to rotate</param>
    /// <returns>Returns the rotated point</returns>
    public MPoint Rotate(double degrees, double centerX, double centerY)
    {
        // translate this point back to the center
        var newX = X - centerX;
        var newY = Y - centerY;

        // rotate the values
        var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

        // translate back to original reference frame
        newX = p.X + centerX;
        newY = p.Y + centerY;

        return new MPoint(newX, newY);
    }

    /// <summary>
    ///     Calculates a new point by rotating this point clockwise about the specified center point
    /// </summary>
    /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
    /// <param name="center">MPoint about which to rotate</param>
    /// <returns>Returns the rotated point</returns>
    public MPoint Rotate(double degrees, MPoint center)
    {
        return Rotate(degrees, center.X, center.Y);
    }
}
