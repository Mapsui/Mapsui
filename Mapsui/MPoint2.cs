using Mapsui.Utilities;

namespace Mapsui;

public class MPoint2
{
    public MPoint2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public MPoint2(MPoint2 point)
    {
        X = point.X;
        Y = point.Y;
    }
    public double X { get; set; }
    public double Y { get; set; }
    public MRect MRect => new MRect(X, Y, X, Y);

    public MPoint2 Copy()
    {
        return new MPoint2(X, Y);
    }

    public double Distance(MPoint2 point)
    {
        return Algorithms.Distance(X, Y, point.X, point.Y);
    }

    public bool Equals(MPoint2? point)
    {
        if (point == null) return false;
        return X == point.X && Y == point.Y;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

    public MPoint2 Offset(double offsetX, double offsetY)
    {
        return new MPoint2(X + offsetX, Y + offsetY);
    }

    /// <summary>
    ///     Calculates a new point by rotating this point clockwise about the specified center point
    /// </summary>
    /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
    /// <param name="centerX">X coordinate of point about which to rotate</param>
    /// <param name="centerY">Y coordinate of point about which to rotate</param>
    /// <returns>Returns the rotated point</returns>
    public MPoint2 Rotate(double degrees, double centerX, double centerY)
    {
        // translate this point back to the center
        var newX = X - centerX;
        var newY = Y - centerY;

        // rotate the values
        var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

        // translate back to original reference frame
        newX = p.X + centerX;
        newY = p.Y + centerY;

        return new MPoint2(newX, newY);
    }

    /// <summary>
    ///     Calculates a new point by rotating this point clockwise about the specified center point
    /// </summary>
    /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
    /// <param name="center">MPoint about which to rotate</param>
    /// <returns>Returns the rotated point</returns>
    public MPoint2 Rotate(double degrees, MPoint2 center)
    {
        return Rotate(degrees, center.X, center.Y);
    }

    public static MPoint2 operator +(MPoint2 point1, MPoint2 point2)
    {
        return new MPoint2(point1.X + point2.X, point1 .Y + point2.Y);
    }

    public static MPoint2 operator -(MPoint2 point1, MPoint2 point2)
    {
        return new MPoint2(point1.X - point2.X, point1.Y - point2.Y);
    }

    public static MPoint2 operator *(MPoint2 point1, double multiplier)
    {
        return new MPoint2(point1.X * multiplier, point1.Y * multiplier);
    }
}
