// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;

namespace Mapsui.Geometries
{
    /// <summary>
    /// A LinearRing is a LineString that is both closed and simple.
    /// </summary>
    public class LinearRing : LineString
    {
        /// <summary>
        /// Initializes an instance of a LinearRing from a set of vertices
        /// </summary>
        /// <param name="vertices"></param>
        public LinearRing(IList<Point> vertices)
            : base(vertices)
        {
        }

        /// <summary>
        /// Initializes an instance of a LinearRing
        /// </summary>
        public LinearRing()
        {
        }

        /// <summary>
        /// Initializes an instance of a LinearRing
        /// </summary>
        /// <param name="points"></param>
        public LinearRing(IEnumerable<double[]> points)
            : base(points)
        {
        }

        /// <summary>
        /// Returns the area of the LinearRing
        /// </summary>
        public double Area
        {
            get
            {
                if (Vertices.Count < 3)
                    return 0;
                double sum = 0;
                double ax = Vertices[0].X;
                double ay = Vertices[0].Y;
                for (int i = 1; i < Vertices.Count - 1; i++)
                {
                    double bx = Vertices[i].X;
                    double by = Vertices[i].Y;
                    double cx = Vertices[i + 1].X;
                    double cy = Vertices[i + 1].Y;
                    sum += ax*by - ay*bx +
                           ay*cx - ax*cy +
                           bx*cy - cx*by;
                }
                return Math.Abs(-sum/2);
            }
        }

        /// <summary>
        /// Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new LinearRing Clone()
        {
            var l = new LinearRing();
            for (int i = 0; i < Vertices.Count; i++)
                l.Vertices.Add(Vertices[i].Clone());
            return l;
        }

        /// <summary>
        /// Tests whether a ring is oriented counter-clockwise.
        /// </summary>
        /// <returns>Returns true if ring is oriented counter-clockwise.</returns>
        public bool IsCCW()
        {
            int hii, i;
            int nPts = Vertices.Count;

            // check that this is a valid ring - if not, simply return a dummy value
            if (nPts < 4) return false;

            // algorithm to check if a Ring is stored in CCW order
            // find highest point
            Point hip = Vertices[0];
            hii = 0;
            for (i = 1; i < nPts; i++)
            {
                Point p = Vertices[i];
                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }
            // find points on either side of highest
            int iPrev = hii - 1;
            if (iPrev < 0) iPrev = nPts - 2;
            int iNext = hii + 1;
            if (iNext >= nPts) iNext = 1;
            Point prev = Vertices[iPrev];
            Point next = Vertices[iNext];
            // translate so that hip is at the origin.
            // This will not affect the area calculation, and will avoid
            // finite-accuracy errors (i.e very small vectors with very large coordinates)
            // This also simplifies the discriminant calculation.
            double prev2X = prev.X - hip.X;
            double prev2Y = prev.Y - hip.Y;
            double next2X = next.X - hip.X;
            double next2Y = next.Y - hip.Y;
            // compute cross-product of vectors hip->next and hip->prev
            // (e.g. area of parallelogram they enclose)
            double disc = next2X*prev2Y - next2Y*prev2X;
            // If disc is exactly 0, lines are collinear.  There are two possible cases:
            //	(1) the lines lie along the x axis in opposite directions
            //	(2) the line lie on top of one another
            //  (2) should never happen, so we're going to ignore it!
            //	(Might want to assert this)
            //  (1) is handled by checking if next is left of prev ==> CCW

            if (disc == 0.0)
            {
                // poly is CCW if prev x is right of next x
                return (prev.X > next.X);
            }
            // if area is positive, points are ordered CCW
            return (disc > 0.0);
        }

        /// <summary>
        /// Returns true of the Point 'p' is within the instance of this ring
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsPointWithin(Point p)
        {
            bool c = false;
            for (int i = 0, j = Vertices.Count - 1; i < Vertices.Count; j = i++)
            {
                if ((((Vertices[i].Y <= p.Y) && (p.Y < Vertices[j].Y)) ||
                     ((Vertices[j].Y <= p.Y) && (p.Y < Vertices[i].Y))) &&
                    (p.X <
                     (Vertices[j].X - Vertices[i].X)*(p.Y - Vertices[i].Y)/(Vertices[j].Y - Vertices[i].Y) +
                     Vertices[i].X))
                    c = !c;
            }
            return c;
        }
    }
}