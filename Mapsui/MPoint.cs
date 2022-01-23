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
using Mapsui.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui
{
    /// <summary>
    ///     A MPoint is a 0-dimensional geometry and represents a single location in 2D coordinate space. A MPoint has a x
    ///     coordinate
    ///     value and a y-coordinate value. 
    /// </summary>
    public class MPoint
    {
        /// <summary>
        ///     Initializes a new MPoint
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public MPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        public MPoint(MPoint point)
        {
            X = point.X;
            Y = point.Y;
        }

        public MPoint() : this(0, 0)
        {
        }

        /// <summary>
        ///     Create a new point by a double[] array
        /// </summary>
        /// <param name="point"></param>
        public MPoint(double[] point)
        {
            if (point.Length != 2)
                throw new Exception("Only 2 dimensions are supported for points");

            X = point[0];
            Y = point[1];
        }

        /// <summary>
        ///     Gets or sets the X coordinate of the point
        /// </summary>
        public double X { get; set; }

        /// <summary>
        ///     Gets or sets the Y coordinate of the point
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        ///     Returns part of coordinate. Index 0 = X, Index 1 = Y
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual double this[uint index]
        {
            get
            {
                if (index == 0)
                    return X;
                if
                    (index == 1)
                    return Y;
                throw new Exception("MPoint index out of bounds");
            }
            set
            {
                if (index == 0)
                    X = value;
                else if (index == 1)
                    Y = value;
                else
                    throw new Exception("MPoint index out of bounds");
            }
        }

        /// <summary>
        ///     Returns the number of ordinates for this point
        /// </summary>
        public virtual int NumOrdinates => 2;

        /// <summary>
        ///     Comparator used for ordering point first by ascending X, then by ascending Y.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(MPoint other)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((X < other.X) || ((X == other.X) && (Y < other.Y)))
                return -1;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((X > other.X) || ((X == other.X) && (Y > other.Y)))
                return 1;

            return 0;
        }

        /// <summary>
        ///     exports a point into a 2-dimensional double array
        /// </summary>
        /// <returns></returns>
        public double[] ToDoubleArray()
        {
            return new[] { X, Y };
        }

        /// <summary>
        ///     Returns a point based on degrees, minutes and seconds notation.
        ///     For western or southern coordinates, add minus '-' in front of all longitude and/or latitude values
        /// </summary>
        /// <param name="longDegrees">Longitude degrees</param>
        /// <param name="longMinutes">Longitude minutes</param>
        /// <param name="longSeconds">Longitude seconds</param>
        /// <param name="latDegrees">Latitude degrees</param>
        /// <param name="latMinutes">Latitude minutes</param>
        /// <param name="latSeconds">Latitude seconds</param>
        /// <returns>MPoint</returns>
        public static MPoint FromDMS(double longDegrees, double longMinutes, double longSeconds,
            double latDegrees, double latMinutes, double latSeconds)
        {
            return new MPoint(longDegrees + longMinutes / 60 + longSeconds / 3600,
                latDegrees + latMinutes / 60 + latSeconds / 3600);
        }

        /// <summary>
        ///     Returns a 2D <see cref="MPoint" /> instance from this
        /// </summary>
        /// <returns>
        ///     <see cref="MPoint" />
        /// </returns>
        public MPoint AsPoint()
        {
            return new MPoint(X, Y);
        }

        /// <summary>
        ///     This method must be overridden using 'public new [derived_data_type] Clone()'
        /// </summary>
        /// <returns>Clone</returns>
        public MPoint Clone()
        {
            return new MPoint(X, Y);
        }

        /// <summary>
        ///     Vector + Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns></returns>
        public static MPoint operator +(MPoint v1, MPoint v2)
        {
            return new MPoint(v1.X + v2.X, v1.Y + v2.Y);
        }

        /// <summary>
        ///     Vector - Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns>Cross product</returns>
        public static MPoint operator -(MPoint v1, MPoint v2)
        {
            return new MPoint(v1.X - v2.X, v1.Y - v2.Y);
        }

        /// <summary>
        ///     Vector * Scalar
        /// </summary>
        /// <param name="m">Vector</param>
        /// <param name="d">Scalar (double)</param>
        /// <returns></returns>
        public static MPoint operator *(MPoint m, double d)
        {
            return new MPoint(m.X * d, m.Y * d);
        }

        /// <summary>
        ///     Checks whether this instance is spatially equal to the MPoint 'o'
        /// </summary>
        /// <param name="p">MPoint to compare to</param>
        /// <returns></returns>
        public virtual bool Equals(MPoint? p)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (p != null) && (p.X == X) && (p.Y == Y);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="GetHashCode" /> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode" />.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        ///     Returns the distance between this geometry instance and another geometry, as
        ///     measured in the spatial reference system of this instance.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double Distance(MPoint point)
        {
            return Math.Sqrt(Math.Pow(X - point.X, 2) + Math.Pow(Y - point.Y, 2));
        }

        /// <summary>
        ///     Returns the distance between this point and a <see cref="Mapsui.MRect" />
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public double Distance(MRect box)
        {
            return box.Distance(this);
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns></returns>
        public MRect MRect => new(X, Y, X, Y);

        /// <summary>
        ///     Calculates a new point by rotating this point clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="centerX">X coordinate of point about which to rotate</param>
        /// <param name="centerY">Y coordinate of point about which to rotate</param>
        /// <returns>Returns the rotated point</returns>
        public MPoint Rotate(double degrees, double centerX, double centerY)
        {
            // translate this point back to the center
            var newX = X - centerX;
            var newY = Y - centerY;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            newX = p.X + centerX;
            newY = p.Y + centerY;

            return new MPoint(newX, newY);
        }

        /// <summary>
        ///     Calculates a new point by rotating this point clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="center">MPoint about which to rotate</param>
        /// <returns>Returns the rotated point</returns>
        public MPoint Rotate(double degrees, MPoint center)
        {
            return Rotate(degrees, center.X, center.Y);
        }

        /// <summary>
        ///     Calculates a new point by translating this point by the specified offset
        /// </summary>
        /// <param name="offsetX">Offset to translate in X axis</param>
        /// <param name="offsetY">Offset to translate in Y axis</param>
        /// <returns>Returns the offset point</returns>
        public MPoint Offset(double offsetX, double offsetY)
        {
            return new MPoint(X + offsetX, Y + offsetY);
        }

        public bool Contains(MPoint point)
        {
            return false;
        }
    }
}