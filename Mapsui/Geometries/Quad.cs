// Copyright 2014 Scott Dewald
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
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     Double precision polygon with 4 explicit vertices. This is useful to represent
    ///     a BoundingBox that has been rotated.
    /// </summary>
    /// <remarks>
    ///     The sides do not have to be parallel to the two axes of the coordinate system.
    ///     If this has been rotated, the 'BottomLeft' vertex may not actually be the min point in x/y.
    /// </remarks>
    public class Quad : IEquatable<Quad>
    {
        public Quad()
        {
            BottomLeft = new Point();
            TopLeft = new Point();
            TopRight = new Point();
            BottomRight = new Point();
        }

        public Quad(Quad quad)
        {
            BottomLeft = quad.BottomLeft;
            TopLeft = quad.TopLeft;
            TopRight = quad.TopRight;
            BottomRight = quad.BottomRight;
        }

        public Quad(Point bottomLeft, Point topLeft, Point topRight, Point bottomRight)
        {
            BottomLeft = bottomLeft;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
        }

        public Point BottomLeft { get; set; }
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomRight { get; set; }

        /// <summary>
        ///     Returns the vertices in clockwise order from bottom left around to bottom right
        /// </summary>
        public IEnumerable<Point> Vertices
        {
            get
            {
                yield return BottomLeft;
                yield return TopLeft;
                yield return TopRight;
                yield return BottomRight;
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">Other object to compare</param>
        /// <returns>Returns true if they are equal</returns>
        public bool Equals(Quad other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return Equals(BottomLeft, other.BottomLeft) &&
                   Equals(TopLeft, other.TopLeft) &&
                   Equals(TopRight, other.TopRight) &&
                   Equals(BottomRight, other.BottomRight);
        }

        /// <summary>
        ///     Creates a new quad by rotate all 4 vertices clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="centerX">X coordinate of point about which to rotate</param>
        /// <param name="centerY">Y coordinate of point about which to rotate</param>
        /// <returns>Returns the rotate quad</returns>
        public Quad Rotate(double degrees, double centerX, double centerY)
        {
            var newBottomLeft = BottomLeft.Rotate(degrees, centerX, centerY);
            var newTopLeft = TopLeft.Rotate(degrees, centerX, centerY);
            var newTopRight = TopRight.Rotate(degrees, centerX, centerY);
            var newBottomRight = BottomRight.Rotate(degrees, centerX, centerY);

            return new Quad(newBottomLeft, newTopLeft, newTopRight, newBottomRight);
        }

        /// <summary>
        ///     Calculates a new bounding box that encompasses all 4 vertices.
        /// </summary>
        /// <returns>Returns the calculate bounding box</returns>
        public BoundingBox ToBoundingBox()
        {
            var minX = Vertices.Select(p => p.X).Min();
            var minY = Vertices.Select(p => p.Y).Min();
            var maxX = Vertices.Select(p => p.X).Max();
            var maxY = Vertices.Select(p => p.Y).Max();

            return new BoundingBox(minX, minY, maxX, maxY);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">Other object to compare</param>
        /// <returns>Returns true if they are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Quad);
        }

        /// <summary>
        ///     Returns a hash code for the specified object
        /// </summary>
        /// <returns>A hash code for the specified object</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BottomLeft != null ? BottomLeft.GetHashCode() : 0;
                hashCode = (hashCode*397) ^ (TopLeft != null ? TopLeft.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (TopRight != null ? TopRight.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (BottomRight != null ? BottomRight.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Returns a string representation of the vertices from bottom-left clockwise to bottom-right
        /// </summary>
        /// <returns>Returns the string</returns>
        public override string ToString()
        {
            return string.Format("BL: {0}  TL: {1}  TR: {2}  BR: {3}",
                ToString(BottomLeft), ToString(TopLeft), ToString(TopRight), ToString(BottomRight));
        }

        private static string ToString(Point p)
        {
            if (p == null)
                return "";
            if (p.IsEmpty())
                return "Empty";

            return string.Format("({0}, {1})", p.X, p.Y);
        }
    }
}