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

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui
{
    /// <summary>
    ///     Bounding box type with double precision
    /// </summary>
    /// <remarks>
    ///     The Bounding Box represents a box whose sides are parallel to the two axes of the coordinate system.
    /// </remarks>
    public class MRect : IEquatable<MRect>
    {
        public MRect(MRect rect) : this(
            rect.Min.X,
            rect.Min.Y,
            rect.Max.X,
            rect.Max.Y)
        { }

        /// <summary>
        ///     Initializes a rect box
        /// </summary>
        /// <remarks>
        ///     In case min values are larger than max values, the parameters will be swapped to ensure correct min/max boundary
        /// </remarks>
        /// <param name="minX">left</param>
        /// <param name="minY">bottom</param>
        /// <param name="maxX">right</param>
        /// <param name="maxY">top</param>
        public MRect(double minX, double minY, double maxX, double maxY)
        {
            Min = new MPoint(minX, minY);
            Max = new MPoint(maxX, maxY);
            CheckMinMax();
        }

        /// <summary>
        ///     Initializes a new Bounding Box based on the bounds from a set of bounding boxes
        /// </summary>
        public MRect(IEnumerable<MRect> rects)
        {
            foreach (var rect in rects)
            {
                Min ??= rect.Min.Clone();
                Max ??= rect.Max.Clone();

                Min.X = Math.Min(rect.Min.X, Min.X);
                Min.Y = Math.Min(rect.Min.Y, Min.Y);
                Max.X = Math.Max(rect.Max.X, Max.X);
                Max.Y = Math.Max(rect.Max.Y, Max.Y);
            }

            if (Min == null) throw new ArgumentException("Empty Collection", nameof(rects));
            if (Max == null) throw new ArgumentException("Empty Collection", nameof(rects));
        }

        public double MinX => Min.X;

        public double MinY => Min.Y;

        public double MaxX => Max.X;

        public double MaxY => Max.Y;

        /// <summary>
        ///     Gets or sets the lower left corner.
        /// </summary>
        public MPoint Min { get; set; }

        /// <summary>
        ///     Gets or sets the upper right corner.
        /// </summary>
        public MPoint Max { get; set; }

        /// <summary>
        ///     Gets the left boundary
        /// </summary>
        public double Left => Min.X;

        /// <summary>
        ///     Gets the right boundary
        /// </summary>
        public double Right => Max.X;

        /// <summary>
        ///     Gets the top boundary
        /// </summary>
        public double Top => Max.Y;

        /// <summary>
        ///     Gets the bottom boundary
        /// </summary>
        public double Bottom => Min.Y;

        public MPoint TopLeft => new(Left, Top);

        public MPoint TopRight => new(Right, Top);

        public MPoint BottomLeft => new(Left, Bottom);

        public MPoint BottomRight => new(Right, Bottom);

        /// <summary>
        ///     Returns the width of the rect
        /// </summary>
        /// <returns>Width of rect</returns>
        public double Width => Math.Abs(Max.X - Min.X);

        /// <summary>
        ///     Returns the height of the rect
        /// </summary>
        /// <returns>Height of rect</returns>
        public double Height => Math.Abs(Max.Y - Min.Y);

        /// <summary>
        ///     Intersection scalar (used for weighting in building the tree)
        /// </summary>
        public uint LongestAxis
        {
            get
            {
                var boxdim = Max - Min;
                uint la = 0; // longest axis
                double lav = 0; // longest axis length
                // for each dimension  
                for (uint ii = 0; ii < 2; ii++)
                {
                    // check if its longer
                    if (boxdim[ii] > lav)
                    {
                        // store it if it is
                        la = ii;
                        lav = boxdim[ii];
                    }
                }
                return la;
            }
        }

        /// <summary>
        ///     Checks whether the values of this instance is equal to the values of another instance.
        /// </summary>
        /// <param name="other"><see cref="MRect" /> to compare to.</param>
        /// <returns>True if equal</returns>
        public bool Equals(MRect? other)
        {
            if (other == null) return false;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (Left == other.Left) && (Right == other.Right) && (Top == other.Top) && (Bottom == other.Bottom);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///     Checks whether min values are actually smaller than max values and in that case swaps them.
        /// </summary>
        /// <returns>true if the rect was changed</returns>
        public bool CheckMinMax()
        {
            var wasSwapped = false;
            if (Min.X > Max.X)
            {
                (Min.X, Max.X) = (Max.X, Min.X);
                wasSwapped = true;
            }
            if (Min.Y > Max.Y)
            {
                (Min.Y, Max.Y) = (Max.Y, Min.Y);
                wasSwapped = true;
            }
            return wasSwapped;
        }

        /// <summary>
        ///     Determines whether the rect intersects another rect
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool Intersects(MRect? box)
        {
            if (box == null) return false;
            return !((box.Min.X > Max.X) ||
                     (box.Max.X < Min.X) ||
                     (box.Min.Y > Max.Y) ||
                     (box.Max.Y < Min.Y));
        }

        /// <summary>
        ///     Returns true if this instance contains the <see cref="MRect" />
        /// </summary>
        /// <param name="r">
        ///     <see cref="MRect" />
        /// </param>
        /// <returns>True it contains</returns>
        public bool Contains(MRect r)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if ((Min[cIndex] > r.Min[cIndex]) || (Max[cIndex] < r.Max[cIndex])) return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns the area of the MRect
        /// </summary>
        /// <returns>Area of box</returns>
        public double GetArea()
        {
            return Width * Height;
        }

        /// <summary>
        ///     Computes the joined rect of this instance and another rect
        /// </summary>
        /// <param name="box">Rect to join with</param>
        /// <returns>Rect containing both rects</returns>
        public MRect Join(MRect? box)
        {
            if (box == null)
                return Clone();
            return new MRect(Math.Min(Min.X, box.Min.X), Math.Min(Min.Y, box.Min.Y),
                Math.Max(Max.X, box.Max.X), Math.Max(Max.Y, box.Max.Y));
        }

        /// <summary>
        ///     Increases the size of the rect by the given amount in all directions
        /// </summary>
        /// <param name="amount">Amount to grow in all directions</param>
        public MRect Grow(double amount)
        {
            var box = Clone();
            box.Min.X -= amount;
            box.Min.Y -= amount;
            box.Max.X += amount;
            box.Max.Y += amount;
            box.CheckMinMax();
            return box;
        }

        /// <summary>
        ///     Increases the size of the MRect by the given amount in horizontal and vertical directions
        /// </summary>
        /// <param name="amountInX">Amount to grow in horizontal direction</param>
        /// <param name="amountInY">Amount to grow in vertical direction</param>
        public MRect Grow(double amountInX, double amountInY)
        {
            var box = Clone();
            box.Min.X -= amountInX;
            box.Min.Y -= amountInY;
            box.Max.X += amountInX;
            box.Max.Y += amountInY;
            box.CheckMinMax();
            return box;
        }

        /// <summary>
        /// Adjusts the size by increasing Width and Heigh with (Width * Height) / 2 * factor.
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public MRect Multiply(double factor)
        {
            if (factor < 0)
            {
                throw new ArgumentException($"{nameof(factor)} can not be smaller than zero");
            }

            var size = (Width + Height) * 0.5;
            var change = (size * 0.5 * factor) - (size * 0.5);
            var box = Clone();
            box.Min.X -= change;
            box.Min.Y -= change;
            box.Max.X += change;
            box.Max.Y += change;
            return box;
        }

        /// <summary>
        ///     Calculates a new quad by rotating this rect about its center by the
        ///     specified angle clockwise
        /// </summary>
        /// <param name="degrees">Angle about which to rotate (degrees)</param>
        /// <returns>Returns the calculated quad</returns>
        public MQuad Rotate(double degrees)
        {
            var bottomLeft = new MPoint(MinX, MinY);
            var topLeft = new MPoint(MinX, MaxY);
            var topRight = new MPoint(MaxX, MaxY);
            var bottomRight = new MPoint(MaxX, MinY);
            var quad = new MQuad(bottomLeft, topLeft, topRight, bottomRight);
            var center = Centroid;

            return quad.Rotate(degrees, center.X, center.Y);
        }

        /// <summary>
        ///     Checks whether a point lies within the rect
        /// </summary>
        /// <param name="p">MPoint</param>
        /// <returns>true if point is within</returns>
        public bool Contains(MPoint? p)
        {
            if (p == null)
                return false;
            if (Max.X < p.X)
                return false;
            if (Min.X > p.X)
                return false;
            if (Max.Y < p.Y)
                return false;
            if (Min.Y > p.Y)
                return false;
            return true;
        }

        /// <summary>
        ///     Returns the center of the rect
        /// </summary>
        public MPoint Centroid => (Min + Max) * .5;

        /// <summary>
        ///     Creates a copy of the MRect
        /// </summary>
        /// <returns></returns>
        public MRect Clone()
        {
            return new MRect(Min.X, Min.Y, Max.X, Max.Y);
        }

        /// <summary>
        ///     Returns the vertices in clockwise order from bottom left around to bottom right
        /// </summary>
        public IEnumerable<MPoint> Vertices
        {
            get
            {
                yield return BottomLeft;
                yield return TopLeft;
                yield return TopRight;
                yield return BottomRight;
            }
        }
    }
}