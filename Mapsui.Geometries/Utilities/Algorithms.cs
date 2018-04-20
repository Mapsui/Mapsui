// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;

namespace Mapsui.Geometries.Utilities
{
    public static class Algorithms
    {
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

        public static double Distance(Point a, Point b)
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
            return degrees*Math.PI/180.0;
        }

        /// <summary>
        ///     Rotates the specified point clockwise about the origin
        /// </summary>
        /// <param name="x">X coordinate to rotate</param>
        /// <param name="y">Y coordinate to rotate</param>
        /// <param name="degrees">Angle to rotate (degrees)</param>
        /// <returns>Returns the rotated point</returns>
        public static Point RotateClockwiseDegrees(double x, double y, double degrees)
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
        public static Point RotateClockwiseRadians(double x, double y, double radians)
        {
            var cos = Math.Cos(-radians);
            var sin = Math.Sin(-radians);
            var newX = x*cos - y*sin;
            var newY = x*sin + y*cos;

            return new Point(newX, newY);
        }

        // METHOD IsCCW() IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
        /*
		 *  Copyright (C) 2002 Urban Science Applications, Inc. 
		 *
		 *  This library is free software; you can redistribute it and/or
		 *  modify it under the terms of the GNU Lesser General Public
		 *  License as published by the Free Software Foundation; either
		 *  version 2.1 of the License, or (at your option) any later version.
		 *
		 *  This library is distributed in the hope that it will be useful,
		 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
		 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
		 *  Lesser General Public License for more details.
		 *
		 *  You should have received a copy of the GNU Lesser General Public
		 *  License along with this library; if not, write to the Free Software
		 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
		 *
		 */

        /// <summary>
        ///     Tests whether a ring is oriented counter-clockwise.
        /// </summary>
        /// <param name="ring">Ring to test.</param>
        /// <returns>Returns true if ring is oriented counter-clockwise.</returns>
        public static bool IsCCW(LinearRing ring)
        {
            // Check if the ring has enough vertices to be a ring
            if (ring.Vertices.Count < 3) throw new ArgumentException("Invalid LinearRing");

            // find the point with the largest Y coordinate
            var hip = ring.Vertices[0];
            var hii = 0;
            for (var i = 1; i < ring.Vertices.Count; i++)
            {
                var p = ring.Vertices[i];
                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }
            // Point left to Hip
            var iPrev = hii - 1;
            if (iPrev < 0) iPrev = ring.Vertices.Count - 2;
            // Point right to Hip
            var iNext = hii + 1;
            if (iNext >= ring.Vertices.Count) iNext = 1;
            var prevPoint = ring.Vertices[iPrev];
            var nextPoint = ring.Vertices[iNext];

            // translate so that hip is at the origin.
            // This will not affect the area calculation, and will avoid
            // finite-accuracy errors (i.e very small vectors with very large coordinates)
            // This also simplifies the discriminant calculation.
            var prev2X = prevPoint.X - hip.X;
            var prev2Y = prevPoint.Y - hip.Y;
            var next2X = nextPoint.X - hip.X;
            var next2Y = nextPoint.Y - hip.Y;
            // compute cross-product of vectors hip->next and hip->prev
            // (e.g. area of parallelogram they enclose)
            var disc = next2X*prev2Y - next2Y*prev2X;
            // If disc is exactly 0, lines are collinear.  There are two possible cases:
            //	(1) the lines lie along the x axis in opposite directions
            //	(2) the line lie on top of one another
            //  (2) should never happen, so we're going to ignore it!
            //	(Might want to assert this)
            //  (1) is handled by checking if next is left of prev ==> CCW

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (disc == 0.0)
                return prevPoint.X > nextPoint.X;
            // if area is positive, points are ordered CCW
            return disc > 0.0;
        }

        public static bool PointInPolygon(IList<Point> ring, Point point)
        {
            // taken from: http://stackoverflow.com/a/2922778/85325
            var result = false;
            
            for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
            {
                if ((ring[i].Y > point.Y != ring[j].Y > point.Y) &&
                    (point.X < (ring[j].X - ring[i].X)*(point.Y - ring[i].Y)/(ring[j].Y - ring[i].Y) + ring[i].X))
                    result = !result;
            }

            return result;
        }

        public static double DistanceToLine(Point point, IList<Point> points)
        {
            var minDist = Double.MaxValue;

            for (var i = 0; i < points.Count - 1; i++)
            {
                var dist = CGAlgorithms.DistancePointLine(point, points[i], points[i + 1]);
                if (dist < minDist)
                    minDist = dist;
            }

            return minDist;
        }

        /// <summary>
        /// Returns the shortest distance to a line and also the index of the segment 
        /// with that shortest distance. Segments count from zero to vertex count - 1.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static (double Distance, int Segment ) GetDistanceAndSegmentIndex(Point point, IList<Point> points)
        {
            var minDist = Double.MaxValue;
            int segment = 0;

            for (var i = 0; i < points.Count - 1; i++)
            {
                var dist = CGAlgorithms.DistancePointLine(point, points[i], points[i + 1]);
                if (dist < minDist)
                {
                    minDist = dist;
                    segment = i;
                }
            }

            return (minDist, segment);
        }
    }
}