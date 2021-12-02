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
using System.Globalization;

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
        ///     Initializes a bounding box
        /// </summary>
        /// <param name="minPoint">Lower left corner</param>
        /// <param name="maxPoint">Upper right corner</param>
        public MRect(MPoint minPoint, MPoint maxPoint)
            : this(minPoint.X, minPoint.Y, maxPoint.X, maxPoint.Y) { }

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
        ///     Moves/translates the <see cref="MRect" /> along the the specified vector
        /// </summary>
        /// <param name="vector">Offset vector</param>
        public void Offset(MPoint vector)
        {
            Min += vector;
            Max += vector;
        }

        public void Offset(double x, double y)
        {
            Min.X += x;
            Min.Y += y;
            Max.X += x;
            Max.Y += y;
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
        ///     Returns true if this instance touches the <see cref="MRect" />
        /// </summary>
        /// <param name="r">
        ///     <see cref="MRect" />
        /// </param>
        /// <returns>True it touches</returns>
        public bool Touches(MRect r)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if (Min[cIndex] >= r.Min[cIndex] && Min[cIndex] <= r.Min[cIndex] ||
                    Max[cIndex] >= r.Max[cIndex] && Max[cIndex] <= r.Max[cIndex])
                    return true;
            }
            return false;
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
        ///     Returns true if this instance touches the <see cref="MPoint" />
        /// </summary>
        /// <param name="p">Geometry</param>
        /// <returns>True if touches</returns>
        public bool Touches(MPoint p)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if (((Min[cIndex] > p[cIndex]) && (Min[cIndex] < p[cIndex])) ||
                    ((Max[cIndex] > p[cIndex]) && (Max[cIndex] < p[cIndex])))
                    return true;
            }
            return false;
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
        ///     Gets the intersecting area between two rects
        /// </summary>
        /// <param name="r">MRect</param>
        /// <returns>Area</returns>
        public double GetIntersectingArea(MRect r)
        {
            uint cIndex;
            for (cIndex = 0; cIndex < 2; cIndex++)
            {
                if ((Min[cIndex] > r.Max[cIndex]) || (Max[cIndex] < r.Min[cIndex])) return 0.0;
            }

            var ret = 1.0;

            for (cIndex = 0; cIndex < 2; cIndex++)
            {
                var f1 = Math.Max(Min[cIndex], r.Min[cIndex]);
                var f2 = Math.Min(Max[cIndex], r.Max[cIndex]);
                ret *= f2 - f1;
            }
            return ret;
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
        ///     Computes the joined rect of two rects
        /// </summary>
        /// <param name="box1"></param>
        /// <param name="box2"></param>
        /// <returns></returns>
        public static MRect? Join(MRect? box1, MRect? box2)
        {
            if ((box1 == null) && (box2 == null))
                return null;
            if (box1 == null)
                return box2!.Clone();
            return box1.Join(box2);
        }

        /// <summary>
        ///     Computes the joined <see cref="MRect" /> of an array of rects.
        /// </summary>
        /// <param name="boxes">Boxes to join</param>
        /// <returns>Combined MRect</returns>
        public static MRect? Join(MRect[]? boxes)
        {
            if (boxes == null) return null;
            if (boxes.Length == 1) return boxes[0];
            var box = boxes[0].Clone();
            for (var i = 1; i < boxes.Length; i++)
            {
                box = box.Join(boxes[i]);
            }
            return box;
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

        public virtual double Distance(MRect box)
        {
            var ret = 0.0;
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                var x = 0.0;

                if (box.Max[cIndex] < Min[cIndex]) x = Math.Abs(box.Max[cIndex] - Min[cIndex]);
                else if (Max[cIndex] < box.Min[cIndex]) x = Math.Abs(box.Min[cIndex] - Max[cIndex]);
                ret += x * x;
            }
            return Math.Sqrt(ret);
        }

        /// <summary>
        ///     Computes the minimum distance between this MRect and a <see cref="MPoint" />
        /// </summary>
        /// <param name="p"><see cref="MPoint" /> to calculate distance to.</param>
        /// <returns>Minimum distance.</returns>
        public double Distance(MPoint p)
        {
            var ret = 0.0;

            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if (p[cIndex] < Min[cIndex]) ret += Math.Pow(Min[cIndex] - p[cIndex], 2.0);
                else if (p[cIndex] > Max[cIndex]) ret += Math.Pow(p[cIndex] - Max[cIndex], 2.0);
            }

            return Math.Sqrt(ret);
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
        ///     Returns a string representation of the rect as LowerLeft + UpperRight formatted as "MinX, MinY, MaxX, MaxY"
        /// </summary>
        /// <returns>MinX,MinY MaxX,MaxY</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "MinX: {0}, MinY: {1}, MaxX: {2}, MaxY: {3}", Min.X, Min.Y, Max.X, Max.Y);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            var box = obj as MRect;
            if (obj == null) return false;
            return Equals(box);
        }

        /// <summary>
        ///     Returns a hash code for the specified object
        /// </summary>
        /// <returns>A hash code for the specified object</returns>
        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
    }
}