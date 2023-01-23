// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;

namespace Mapsui.Utilities;

public static class Algorithms
{
    private const double TO_RADIANS = Math.PI / 180.0;
    private const double TO_DEGREES = 180.0 / Math.PI;

    /// <summary>
    ///     Gets the euclidean distance between two points.
    /// </summary>
    /// <param name="x1">The first point's X coordinate.</param>
    /// <param name="y1">The first point's Y coordinate.</param>
    /// <param name="x2">The second point's X coordinate.</param>
    /// <param name="y2">The second point's Y coordinate.</param>
    /// <returns></returns>
    public static double Distance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
    }

    public static double Distance(MPoint a, MPoint b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2.0) + Math.Pow(a.Y - b.Y, 2.0));
    }

    /// <summary>
    ///     Converts the specified angle from degrees to radians
    /// </summary>
    /// <param name="degrees">Angle to convert (degrees)</param>
    /// <returns>Returns the angle in radians</returns>
    public static double DegreesToRadians(double degrees)
    {
        return degrees * TO_RADIANS;
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * TO_DEGREES;
    }

    /// <summary>
    ///     Rotates the specified point clockwise about the origin
    /// </summary>
    /// <param name="x">X coordinate to rotate</param>
    /// <param name="y">Y coordinate to rotate</param>
    /// <param name="degrees">Angle to rotate (degrees)</param>
    /// <returns>Returns the rotated point</returns>
    public static MPoint RotateClockwiseDegrees(double x, double y, double degrees)
    {
        var radians = DegreesToRadians(degrees);

        return RotateClockwiseRadians(x, y, radians);
    }

    /// <summary>
    ///     Rotates the specified point clockwise about the origin
    /// </summary>
    /// <param name="x">X coordinate to rotate</param>
    /// <param name="y">Y coordinate to rotate</param>
    /// <param name="radians">Angle to rotate (radians)</param>
    /// <returns>Returns the rotated point</returns>
    public static MPoint RotateClockwiseRadians(double x, double y, double radians)
    {
        var cos = Math.Cos(-radians);
        var sin = Math.Sin(-radians);
        var newX = x * cos - y * sin;
        var newY = x * sin + y * cos;

        return new MPoint(newX, newY);
    }

    /// <summary>
    ///     Computes the distance from a point p to a line segment AB.
    ///     Note: NON-ROBUST!
    /// </summary>
    /// <param name="p">The point to compute the distance for.</param>
    /// <param name="a">One point of the line.</param>
    /// <param name="b">Another point of the line (must be different to A).</param>
    /// <returns> The distance from p to line segment AB.</returns>
    public static double DistancePointLine(MPoint p, MPoint a, MPoint b)
    {
        // if start == end, then use pt distance
        if (a.Equals(b))
            return p.Distance(a);

        // otherwise use comp.graphics.algorithms Frequently Asked Questions method
        /*(1)     	      AC dot AB
                    r =   ---------
                          ||AB||^2
         
                    r has the following meaning:
                    r=0 Point = A
                    r=1 Point = B
                    r<0 Point is on the backward extension of AB
                    r>1 Point is on the forward extension of AB
                    0<r<1 Point is interior to AB
        */

        var r = ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y))
                /
                ((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

        if (r <= 0.0) return p.Distance(a);
        if (r >= 1.0) return p.Distance(b);

        /*(2)
                                                                        (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                                                                    s = -----------------------------
                                                                                    Curve^2

                                                                    Then the distance from C to Point = |s|*Curve.
                                                        */

        var s = ((a.Y - p.Y) * (b.X - a.X) - (a.X - p.X) * (b.Y - a.Y))
                /
                ((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

        return Math.Abs(s) * Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
    }
}
