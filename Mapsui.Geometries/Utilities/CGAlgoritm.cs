using System;
using System.Collections.Generic;

namespace Mapsui.Geometries.Utilities
{
    /// <summary>
    ///     Specifies and implements various fundamental Computational Geometric algorithms.
    ///     The algorithms supplied in this class are robust for double-precision floating point.
    /// </summary>
    public static class CGAlgorithms
    {
        /// <summary>
        ///     A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Clockwise = -1;

        /// <summary>
        ///     A value that indicates an orientation of clockwise, or a right turn.
        /// </summary>
        public const int Right = Clockwise;

        /// <summary>
        ///     A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int CounterClockwise = 1;

        /// <summary>
        ///     A value that indicates an orientation of counterclockwise, or a left turn.
        /// </summary>
        public const int Left = CounterClockwise;

        /// <summary>
        ///     A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Collinear = 0;

        /// <summary>
        ///     A value that indicates an orientation of collinear, or no turn (straight).
        /// </summary>
        public const int Straight = Collinear;

        /// <summary>
        ///     Computes the closest point on this line segment to another point.
        /// </summary>
        /// <returns>
        ///     A Coordinate which is the closest point on the line segment to the point p.
        /// </returns>
        public static Point ClosestPoint(Point p, Point lineSegFrom, Point lineSegTo)
        {
            var factor = ProjectionFactor(p, lineSegFrom, lineSegTo);
            if ((factor > 0) && (factor < 1))
                return Project(p, lineSegFrom, lineSegTo);
            var dist0 = lineSegFrom.Distance(p);
            var dist1 = lineSegTo.Distance(p);
            return dist0 < dist1 ? lineSegFrom : lineSegTo;
        }

        /// <summary>
        ///     Compute the projection of a point onto the line determined
        ///     by this line segment.
        ///     Note that the projected point  may lie outside the line segment.
        ///     If this is the case,  the projection factor will lie outside the range [0.0, 1.0].
        /// </summary>
        public static Point Project(Point p, Point lineSegFrom, Point lineSegTo)
        {
            if (p.Equals(lineSegFrom) || p.Equals(lineSegTo))
                return new Point(p.X, p.Y);

            var r = ProjectionFactor(p, lineSegFrom, lineSegTo);
            var coord = new Point
            {
                X = lineSegFrom.X + r*(lineSegTo.X - lineSegFrom.X),
                Y = lineSegFrom.Y + r*(lineSegTo.Y - lineSegFrom.Y)
            };
            return coord;
        }

        /// <summary>
        ///     Compute the projection factor for the projection of the point p
        ///     onto this <c>LineSegment</c>. The projection factor is the constant k
        ///     by which the vector for this segment must be multiplied to
        ///     equal the vector for the projection of p.
        /// </summary>
        /// <returns></returns>
        public static double ProjectionFactor(Point p, Point lineSegFrom, Point lineSegTo)
        {
            if (p.Equals(lineSegFrom)) return 0.0;
            if (p.Equals(lineSegTo)) return 1.0;

            // Otherwise, use comp.graphics.algorithms Frequently Asked Questions method
            /*                    AC dot AB
                        r = ------------
                              ||AB||^2
                        r has the following meaning:
                        r=0 Point = A
                        r=1 Point = B
                        r<0 Point is on the backward extension of AB
                        r>1 Point is on the forward extension of AB
                        0<r<1 Point is interior to AB
            */
            var dx = lineSegTo.X - lineSegFrom.X;
            var dy = lineSegTo.Y - lineSegFrom.Y;
            var len2 = dx*dx + dy*dy;
            var r = ((p.X - lineSegFrom.X)*dx + (p.Y - lineSegFrom.Y)*dy)/len2;
            return r;
        }

        /// <summary>
        ///     Computes the distance from a point p to a line segment AB.
        ///     Note: NON-ROBUST!
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="a">One point of the line.</param>
        /// <param name="b">Another point of the line (must be different to A).</param>
        /// <returns> The distance from p to line segment AB.</returns>
        public static double DistancePointLine(Point p, Point a, Point b)
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

            var r = ((p.X - a.X)*(b.X - a.X) + (p.Y - a.Y)*(b.Y - a.Y))
                    /
                    ((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));

            if (r <= 0.0) return p.Distance(a);
            if (r >= 1.0) return p.Distance(b);

/*(2)
                                                                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                                                            s = -----------------------------
                                                                            Curve^2
                                    
                                                            Then the distance from C to Point = |s|*Curve.
                                                */

            var s = ((a.Y - p.Y)*(b.X - a.X) - (a.X - p.X)*(b.Y - a.Y))
                    /
                    ((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));

            return Math.Abs(s)*Math.Sqrt((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));
        }

        /// <summary>
        ///     Computes the perpendicular distance from a point p
        ///     to the (infinite) line containing the points AB
        /// </summary>
        /// <param name="p">The point to compute the distance for.</param>
        /// <param name="a">One point of the line.</param>
        /// <param name="b">Another point of the line (must be different to A).</param>
        /// <returns>The perpendicular distance from p to line AB.</returns>
        public static double DistancePointLinePerpendicular(Point p, Point a, Point b)
        {
            // use comp.graphics.algorithms Frequently Asked Questions method
            /*(2)
                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = -----------------------------
                                         Curve^2

                        Then the distance from C to Point = |s|*Curve.
            */

            var s = ((a.Y - p.Y)*(b.X - a.X) - (a.X - p.X)*(b.Y - a.Y))
                    /
                    ((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));

            return Math.Abs(s)*Math.Sqrt((b.X - a.X)*(b.X - a.X) + (b.Y - a.Y)*(b.Y - a.Y));
        }

        /// <summary>
        ///     Computes the distance from a line segment AB to a line segment CD.
        ///     Note: NON-ROBUST!
        /// </summary>
        /// <param name="a">A point of one line.</param>
        /// <param name="b">The second point of the line (must be different to A).</param>
        /// <param name="c">One point of the line.</param>
        /// <param name="d">Another point of the line (must be different to A).</param>
        /// <returns>The distance from line segment AB to line segment CD.</returns>
        public static double DistanceLineLine(Point a, Point b, Point c, Point d)
        {
            // check for zero-length segments
            if (a.Equals(b))
                return DistancePointLine(a, c, d);
            if (c.Equals(d))
                return DistancePointLine(d, a, b);

            // AB and CD are line segments
            /* from comp.graphics.algo

                Solving the above for r and s yields
                            (Ay-Cy)(Dx-Cx)-(Ax-Cx)(Dy-Cy)
                        r = ----------------------------- (eqn 1)
                            (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)

                            (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                        s = ----------------------------- (eqn 2)
                            (Bx-Ax)(Dy-Cy)-(By-Ay)(Dx-Cx)
                Let Point be the position vector of the intersection point, then
                    Point=A+r(B-A) or
                    Px=Ax+r(Bx-Ax)
                    Py=Ay+r(By-Ay)
                By examining the values of r & s, you can also determine some other
                limiting conditions:
                    If 0<=r<=1 & 0<=s<=1, intersection exists
                    r<0 or r>1 or s<0 or s>1 line segments do not intersect
                    If the denominator in eqn 1 is zero, AB & CD are parallel
                    If the numerator in eqn 1 is also zero, AB & CD are collinear.

            */
            var rTop = (a.Y - c.Y)*(d.X - c.X) - (a.X - c.X)*(d.Y - c.Y);
            var rBottom = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            var sTop = (a.Y - c.Y)*(b.X - a.X) - (a.X - c.X)*(b.Y - a.Y);
            var sBottom = (b.X - a.X)*(d.Y - c.Y) - (b.Y - a.Y)*(d.X - c.X);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((rBottom == 0) || (sBottom == 0))

                return Math.Min(DistancePointLine(a, c, d),
                    Math.Min(DistancePointLine(b, c, d),
                        Math.Min(DistancePointLine(c, a, b),
                            DistancePointLine(d, a, b))));
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var s = sTop/sBottom;
            var r = rTop/rBottom;

            if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
                //no intersection
                return Math.Min(DistancePointLine(a, c, d),
                    Math.Min(DistancePointLine(b, c, d),
                        Math.Min(DistancePointLine(c, a, b),
                            DistancePointLine(d, a, b))));

            return 0.0; //intersection exists
        }

        /// <summary>
        ///     Returns the signed area for a ring.  The area is positive ifthe ring is oriented CW.
        /// </summary>
        /// <param name="ring"></param>
        /// <returns></returns>
        public static double SignedArea(Point[] ring)
        {
            if (ring.Length < 3)
                return 0.0;

            var sum = 0.0;
            for (var i = 0; i < ring.Length - 1; i++)
            {
                var bx = ring[i].X;
                var by = ring[i].Y;
                var cx = ring[i + 1].X;
                var cy = ring[i + 1].Y;
                sum += (bx + cx)*(cy - by);
            }
            return -sum/2.0;
        }

        /// <summary>
        ///     Computes the length of a linestring specified by a sequence of points.
        /// </summary>
        /// <param name="pts">The points specifying the linestring.</param>
        /// <returns>The length of the linestring.</returns>
        public static double Length(IList<Point> pts)
        {
            if (pts.Count < 1)
                return 0.0;

            var sum = 0.0;
            for (var i = 1; i < pts.Count; i++)
            {
                sum += pts[i].Distance(pts[i - 1]);
            }

            return sum;
        }
    }
}