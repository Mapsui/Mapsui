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
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     Bounding box type with double precision
    /// </summary>
    /// <remarks>
    ///     The Bounding Box represents a box whose sides are parallel to the two axes of the coordinate system.
    /// </remarks>
    public class BoundingBox : IEquatable<BoundingBox>
    {
        public BoundingBox() {}

        public BoundingBox(BoundingBox boundingBox) : this(
            boundingBox.Min.X,
            boundingBox.Min.Y,
            boundingBox.Max.X,
            boundingBox.Max.Y) {}

        /// <summary>
        ///     Initializes a bounding box
        /// </summary>
        /// <remarks>
        ///     In case min values are larger than max values, the parameters will be swapped to ensure correct min/max boundary
        /// </remarks>
        /// <param name="minX">left</param>
        /// <param name="minY">bottom</param>
        /// <param name="maxX">right</param>
        /// <param name="maxY">top</param>
        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            Min = new Point(minX, minY);
            Max = new Point(maxX, maxY);
            CheckMinMax();
        }

        /// <summary>
        ///     Initializes a bounding box
        /// </summary>
        /// <param name="minPoint">Lower left corner</param>
        /// <param name="maxPoint">Upper right corner</param>
        public BoundingBox(Point minPoint, Point maxPoint)
            : this(minPoint.X, minPoint.Y, maxPoint.X, maxPoint.Y) {}

        /// <summary>
        ///     Initializes a new Bounding Box based on the bounds from a set of geometries
        /// </summary>
        /// <param name="objects">list of objects</param>
        public BoundingBox(IEnumerable<Geometry> objects) : this(objects.Select(o => o.BoundingBox)) {}

        /// <summary>
        ///     Initializes a new Bounding Box based on the bounds from a set of bounding boxes
        /// </summary>
        public BoundingBox(IEnumerable<BoundingBox> boundingBoxes)
        {
            Max = null;
            Min = null;

            foreach (var boundingBox in boundingBoxes)
            {
                if (Min == null)
                    Min = boundingBox.Min.Clone();
                if (Max == null)
                    Max = boundingBox.Max.Clone();

                Min.X = Math.Min(boundingBox.Min.X, Min.X);
                Min.Y = Math.Min(boundingBox.Min.Y, Min.Y);
                Max.X = Math.Max(boundingBox.Max.X, Max.X);
                Max.Y = Math.Max(boundingBox.Max.Y, Max.Y);
            }
        }

        public double MinX => Min.X;

        public double MinY => Min.Y;

        public double MaxX => Max.X;

        public double MaxY => Max.Y;

        /// <summary>
        ///     Gets or sets the lower left corner.
        /// </summary>
        public Point Min { get; set; }

        /// <summary>
        ///     Gets or sets the upper right corner.
        /// </summary>
        public Point Max { get; set; }

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

        public Point TopLeft => new Point(Left, Top);

        public Point TopRight => new Point(Right, Top);

        public Point BottomLeft => new Point(Left, Bottom);

        public Point BottomRight => new Point(Right, Bottom);

        /// <summary>
        ///     Returns the width of the bounding box
        /// </summary>
        /// <returns>Width of boundingbox</returns>
        public double Width => Math.Abs(Max.X - Min.X);

        /// <summary>
        ///     Returns the height of the bounding box
        /// </summary>
        /// <returns>Height of boundingbox</returns>
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
        /// <param name="other"><see cref="BoundingBox" /> to compare to.</param>
        /// <returns>True if equal</returns>
        public bool Equals(BoundingBox other)
        {
            if (other == null) return false;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (Left == other.Left) && (Right == other.Right) && (Top == other.Top) && (Bottom == other.Bottom);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///     Moves/translates the <see cref="BoundingBox" /> along the the specified vector
        /// </summary>
        /// <param name="vector">Offset vector</param>
        public void Offset(Point vector)
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
        /// <returns>true if the bounding was changed</returns>
        public bool CheckMinMax()
        {
            var wasSwapped = false;
            if (Min.X > Max.X)
            {
                var tmp = Min.X;
                Min.X = Max.X;
                Max.X = tmp;
                wasSwapped = true;
            }
            if (Min.Y > Max.Y)
            {
                var tmp = Min.Y;
                Min.Y = Max.Y;
                Max.Y = tmp;
                wasSwapped = true;
            }
            return wasSwapped;
        }

        /// <summary>
        ///     Determines whether the boundingbox intersects another boundingbox
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool Intersects(BoundingBox box)
        {
            if (box == null) return false;
            return !((box.Min.X > Max.X) ||
                     (box.Max.X < Min.X) ||
                     (box.Min.Y > Max.Y) ||
                     (box.Max.Y < Min.Y));
        }

        /// <summary>
        ///     Returns true if this instance touches the <see cref="BoundingBox" />
        /// </summary>
        /// <param name="r">
        ///     <see cref="BoundingBox" />
        /// </param>
        /// <returns>True it touches</returns>
        public bool Touches(BoundingBox r)
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
        ///     Returns true if this instance contains the <see cref="BoundingBox" />
        /// </summary>
        /// <param name="r">
        ///     <see cref="BoundingBox" />
        /// </param>
        /// <returns>True it contains</returns>
        public bool Contains(BoundingBox r)
        {
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                if ((Min[cIndex] > r.Min[cIndex]) || (Max[cIndex] < r.Max[cIndex])) return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns true if this instance touches the <see cref="Point" />
        /// </summary>
        /// <param name="p">Geometry</param>
        /// <returns>True if touches</returns>
        public bool Touches(Point p)
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
        ///     Returns the area of the BoundingBox
        /// </summary>
        /// <returns>Area of box</returns>
        public double GetArea()
        {
            return Width*Height;
        }

        /// <summary>
        ///     Gets the intersecting area between two boundingboxes
        /// </summary>
        /// <param name="r">BoundingBox</param>
        /// <returns>Area</returns>
        public double GetIntersectingArea(BoundingBox r)
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
        ///     Computes the joined boundingbox of this instance and another boundingbox
        /// </summary>
        /// <param name="box">Boundingbox to join with</param>
        /// <returns>Boundingbox containing both boundingboxes</returns>
        public BoundingBox Join(BoundingBox box)
        {
            if (box == null)
                return Clone();
            return new BoundingBox(Math.Min(Min.X, box.Min.X), Math.Min(Min.Y, box.Min.Y),
                Math.Max(Max.X, box.Max.X), Math.Max(Max.Y, box.Max.Y));
        }

        /// <summary>
        ///     Computes the joined boundingbox of two boundingboxes
        /// </summary>
        /// <param name="box1"></param>
        /// <param name="box2"></param>
        /// <returns></returns>
        public static BoundingBox Join(BoundingBox box1, BoundingBox box2)
        {
            if ((box1 == null) && (box2 == null))
                return null;
            if (box1 == null)
                return box2.Clone();
            return box1.Join(box2);
        }

        /// <summary>
        ///     Computes the joined <see cref="BoundingBox" /> of an array of boundingboxes.
        /// </summary>
        /// <param name="boxes">Boxes to join</param>
        /// <returns>Combined BoundingBox</returns>
        public static BoundingBox Join(BoundingBox[] boxes)
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
        ///     Increases the size of the boundingbox by the givent amount in all directions
        /// </summary>
        /// <param name="amount">Amount to grow in all directions</param>
        public BoundingBox Grow(double amount)
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
        ///     Increases the size of the boundingbox by the givent amount in horizontal and vertical directions
        /// </summary>
        /// <param name="amountInX">Amount to grow in horizontal direction</param>
        /// <param name="amountInY">Amount to grow in vertical direction</param>
        public BoundingBox Grow(double amountInX, double amountInY)
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
        ///     Calculates a new quad by rotating this bounding box about its center by the
        ///     specified angle clockwise
        /// </summary>
        /// <param name="degrees">Angle about which to rotate (degrees)</param>
        /// <returns>Returns the calculated quad</returns>
        public Quad Rotate(double degrees)
        {
            var bottomLeft = new Point(MinX, MinY);
            var topLeft = new Point(MinX, MaxY);
            var topRight = new Point(MaxX, MaxY);
            var bottomRight = new Point(MaxX, MinY);
            var quad = new Quad(bottomLeft, topLeft, topRight, bottomRight);
            var center = Centroid;

            return quad.Rotate(degrees, center.X, center.Y);
        }

        /// <summary>
        ///     Checks whether a point lies within the bounding box
        /// </summary>
        /// <param name="p">Point</param>
        /// <returns>true if point is within</returns>
        public bool Contains(Point p)
        {
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
        ///     Computes the minimum distance between this and another <see cref="BoundingBox" />.
        ///     The distance between overlapping bounding boxes is 0.  Otherwise, the
        ///     distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <param name="box">Box to calculate distance to</param>
        /// <returns>The distance between this and another <see cref="BoundingBox" />.</returns>
        public virtual double Distance(BoundingBox box)
        {
            var ret = 0.0;
            for (uint cIndex = 0; cIndex < 2; cIndex++)
            {
                var x = 0.0;

                if (box.Max[cIndex] < Min[cIndex]) x = Math.Abs(box.Max[cIndex] - Min[cIndex]);
                else if (Max[cIndex] < box.Min[cIndex]) x = Math.Abs(box.Min[cIndex] - Max[cIndex]);
                ret += x*x;
            }
            return Math.Sqrt(ret);
        }

        /// <summary>
        ///     Computes the minimum distance between this BoundingBox and a <see cref="Point" />
        /// </summary>
        /// <param name="p"><see cref="Point" /> to calculate distance to.</param>
        /// <returns>Minimum distance.</returns>
        public virtual double Distance(Point p)
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
        ///     Returns the center of the bounding box
        /// </summary>
        public Point Centroid => (Min + Max) * .5;

        [Obsolete("Use the Centroid field instead")]
        public Point GetCentroid()
        {
            return Centroid;
        }

        /// <summary>
        ///     Creates a copy of the BoundingBox
        /// </summary>
        /// <returns></returns>
        public BoundingBox Clone()
        {
            return new BoundingBox(Min.X, Min.Y, Max.X, Max.Y);
        }

        /// <summary>
        ///     Returns a string representation of the boundingbox as LowerLeft + UpperRight formatted as "MinX,MinY MaxX,MaxY"
        /// </summary>
        /// <returns>MinX,MinY MaxX,MaxY</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1} {2},{3}", Min.X, Min.Y, Max.X, Max.Y);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var box = obj as BoundingBox;
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