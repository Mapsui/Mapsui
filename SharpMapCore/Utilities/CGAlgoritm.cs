using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMap.Geometries;

namespace SharpMap.Utilities
{
   
        /// <summary>
        /// Specifies and implements various fundamental Computational Geometric algorithms.
        /// The algorithms supplied in this class are robust for double-precision floating point.
        /// </summary>
        public static class CGAlgorithms
        {
            /// <summary> 
            /// A value that indicates an orientation of clockwise, or a right turn.
            /// </summary>
            public const int Clockwise = -1;
            /// <summary> 
            /// A value that indicates an orientation of clockwise, or a right turn.
            /// </summary>
            public const int Right = Clockwise;

            /// <summary>
            /// A value that indicates an orientation of counterclockwise, or a left turn.
            /// </summary>
            public const int CounterClockwise = 1;
            /// <summary>
            /// A value that indicates an orientation of counterclockwise, or a left turn.
            /// </summary>
            public const int Left = CounterClockwise;

            /// <summary>
            /// A value that indicates an orientation of collinear, or no turn (straight).
            /// </summary>
            public const int Collinear = 0;
            /// <summary>
            /// A value that indicates an orientation of collinear, or no turn (straight).
            /// </summary>
            public const int Straight = Collinear;


            /// <summary> 
            /// Computes the closest point on this line segment to another point.
            /// </summary>
            /// <param name="p">The point to find the closest point to.</param>
            /// <returns>
            /// A Coordinate which is the closest point on the line segment to the point p.
            /// </returns>
            public static Point ClosestPoint(Point p, Point LineSegFrom, Point LineSegTo)
            {
                var factor = ProjectionFactor(p, LineSegFrom, LineSegTo);
                if (factor > 0 && factor < 1)
                    return Project(p, LineSegFrom, LineSegTo);
                var dist0 = LineSegFrom.Distance(p);
                var dist1 = LineSegTo.Distance(p);
                return dist0 < dist1 ? LineSegFrom : LineSegTo;
            }

            /// <summary> 
            /// Compute the projection of a point onto the line determined
            /// by this line segment.
            /// Note that the projected point  may lie outside the line segment.  
            /// If this is the case,  the projection factor will lie outside the range [0.0, 1.0].
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static Point Project(Point p, Point LineSegFrom, Point LineSegTo)
            {
                if (p.Equals(LineSegFrom) || p.Equals(LineSegTo))
                    return new Point(p.X, p.Y);

                var r = ProjectionFactor(p, LineSegFrom,  LineSegTo);
                Point coord = new Point { X = LineSegFrom.X + r * (LineSegTo.X - LineSegFrom.X), Y = LineSegFrom.Y + r * (LineSegTo.Y - LineSegFrom.Y) };
                return coord;
            }

            /// <summary>
            /// Compute the projection factor for the projection of the point p
            /// onto this <c>LineSegment</c>. The projection factor is the constant k
            /// by which the vector for this segment must be multiplied to
            /// equal the vector for the projection of p.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static double ProjectionFactor(Point p, Point LineSegFrom, Point LineSegTo)
            {
                if (p.Equals(LineSegFrom)) return 0.0;
                if (p.Equals(LineSegTo)) return 1.0;

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
                var dx = LineSegTo.X - LineSegFrom.X;
                var dy = LineSegTo.Y - LineSegFrom.Y;
                var len2 = dx * dx + dy * dy;
                var r = ((p.X - LineSegFrom.X) * dx + (p.Y - LineSegFrom.Y) * dy) / len2;
                return r;
            }


            /// <summary> 
            /// Computes the distance from a point p to a line segment AB.
            /// Note: NON-ROBUST!
            /// </summary>
            /// <param name="p">The point to compute the distance for.</param>
            /// <param name="A">One point of the line.</param>
            /// <param name="B">Another point of the line (must be different to A).</param>
            /// <returns> The distance from p to line segment AB.</returns>
            public static double DistancePointLine(Point p, Point A, Point B)
            {
                // if start == end, then use pt distance
                if (A.Equals(B))
                    return p.Distance(A);

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

                double r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y))
                            /
                            ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

                if (r <= 0.0) return p.Distance(A);
                if (r >= 1.0) return p.Distance(B);


                /*(2)
                                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                            s = -----------------------------
                                            Curve^2

                            Then the distance from C to Point = |s|*Curve.
                */

                double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y))
                            /
                            ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

                return Math.Abs(s) * Math.Sqrt(((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y)));
            }

            /// <summary> 
            /// Computes the perpendicular distance from a point p
            /// to the (infinite) line containing the points AB
            /// </summary>
            /// <param name="p">The point to compute the distance for.</param>
            /// <param name="A">One point of the line.</param>
            /// <param name="B">Another point of the line (must be different to A).</param>
            /// <returns>The perpendicular distance from p to line AB.</returns>
            public static double DistancePointLinePerpendicular(Point p, Point A, Point B)
            {
                // use comp.graphics.algorithms Frequently Asked Questions method
                /*(2)
                                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
                            s = -----------------------------
                                             Curve^2

                            Then the distance from C to Point = |s|*Curve.
                */

                double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y))
                            /
                            ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

                return Math.Abs(s) * Math.Sqrt(((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y)));
            }

            /// <summary> 
            /// Computes the distance from a line segment AB to a line segment CD.
            /// Note: NON-ROBUST!
            /// </summary>
            /// <param name="A">A point of one line.</param>
            /// <param name="B">The second point of the line (must be different to A).</param>
            /// <param name="C">One point of the line.</param>
            /// <param name="D">Another point of the line (must be different to A).</param>
            /// <returns>The distance from line segment AB to line segment CD.</returns>
            public static double DistanceLineLine(Point A, Point B, Point C, Point D)
            {
                // check for zero-length segments
                if (A.Equals(B))
                    return DistancePointLine(A, C, D);
                if (C.Equals(D))
                    return DistancePointLine(D, A, B);

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
                double r_top = (A.Y - C.Y) * (D.X - C.X) - (A.X - C.X) * (D.Y - C.Y);
                double r_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

                double s_top = (A.Y - C.Y) * (B.X - A.X) - (A.X - C.X) * (B.Y - A.Y);
                double s_bot = (B.X - A.X) * (D.Y - C.Y) - (B.Y - A.Y) * (D.X - C.X);

                if ((r_bot == 0) || (s_bot == 0))
                    return Math.Min(DistancePointLine(A, C, D),
                            Math.Min(DistancePointLine(B, C, D),
                            Math.Min(DistancePointLine(C, A, B),
                            DistancePointLine(D, A, B))));


                double s = s_top / s_bot;
                double r = r_top / r_bot;

                if ((r < 0) || (r > 1) || (s < 0) || (s > 1))
                    //no intersection
                    return Math.Min(DistancePointLine(A, C, D),
                            Math.Min(DistancePointLine(B, C, D),
                            Math.Min(DistancePointLine(C, A, B),
                            DistancePointLine(D, A, B))));

                return 0.0; //intersection exists
            }

            /// <summary>
            /// Returns the signed area for a ring.  The area is positive ifthe ring is oriented CW.
            /// </summary>
            /// <param name="ring"></param>
            /// <returns></returns>
            public static double SignedArea(Point[] ring)
            {
                if (ring.Length < 3)
                    return 0.0;

                double sum = 0.0;
                for (int i = 0; i < ring.Length - 1; i++)
                {
                    double bx = ring[i].X;
                    double by = ring[i].Y;
                    double cx = ring[i + 1].X;
                    double cy = ring[i + 1].Y;
                    sum += (bx + cx) * (cy - by);
                }
                return -sum / 2.0;
            }

            /// <summary> 
            /// Computes the length of a linestring specified by a sequence of points.
            /// </summary>
            /// <param name="pts">The points specifying the linestring.</param>
            /// <returns>The length of the linestring.</returns>
            public static double Length(IList<Point> pts)
            {
                if (pts.Count < 1)
                    return 0.0;

                double sum = 0.0;
                for (int i = 1; i < pts.Count; i++)
                    sum += pts[i].Distance(pts[i - 1]);

                return sum;
            }
        }
}
