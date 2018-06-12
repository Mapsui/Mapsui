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
using Mapsui.Geometries.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     A Point is a 0-dimensional geometry and represents a single location in 2D coordinate space. A Point has a x
    ///     coordinate
    ///     value and a y-coordinate value. The boundary of a Point is the empty set.
    /// </summary>
    public class Point : Geometry, IComparable<Point>
    {
        private bool _isEmpty;
        private double _x;
        private double _y;

        /// <summary>
        ///     Initializes a new Point
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        ///     Initializes a new empty Point
        /// </summary>
        public Point() : this(0, 0)
        {
            _isEmpty = true;
        }

        /// <summary>
        ///     Create a new point by a douuble[] array
        /// </summary>
        /// <param name="point"></param>
        public Point(double[] point)
        {
            if (point.Length != 2)
                throw new Exception("Only 2 dimensions are supported for points");

            _x = point[0];
            _y = point[1];
        }

        /// <summary>
        ///     Sets whether this object is empty
        /// </summary>
        protected bool SetIsEmpty
        {
            set => _isEmpty = value;
        }

        /// <summary>
        ///     Gets or sets the X coordinate of the point
        /// </summary>
        public double X
        {
            get
            {
                if (!_isEmpty)
                    return _x;
                throw new Exception("Point is empty");
            }
            set
            {
                _x = value;
                _isEmpty = false;
            }
        }

        /// <summary>
        ///     Gets or sets the Y coordinate of the point
        /// </summary>
        public double Y
        {
            get
            {
                if (!_isEmpty)
                    return _y;
                throw new Exception("Point is empty");
            }
            set
            {
                _y = value;
                _isEmpty = false;
            }
        }

        /// <summary>
        ///     Returns part of coordinate. Index 0 = X, Index 1 = Y
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual double this[uint index]
        {
            get
            {
                if (_isEmpty)
                    throw new Exception("Point is empty");
                if (index == 0)
                    return X;
                if
                    (index == 1)
                    return Y;
                throw new Exception("Point index out of bounds");
            }
            set
            {
                if (index == 0)
                    X = value;
                else if (index == 1)
                    Y = value;
                else
                    throw new Exception("Point index out of bounds");
                _isEmpty = false;
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
        public virtual int CompareTo(Point other)
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
            return new[] {_x, _y};
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
        /// <returns>Point</returns>
        public static Point FromDMS(double longDegrees, double longMinutes, double longSeconds,
            double latDegrees, double latMinutes, double latSeconds)
        {
            return new Point(longDegrees + longMinutes/60 + longSeconds/3600,
                latDegrees + latMinutes/60 + latSeconds/3600);
        }

        /// <summary>
        ///     Returns a 2D <see cref="Point" /> instance from this
        /// </summary>
        /// <returns>
        ///     <see cref="Point" />
        /// </returns>
        public Point AsPoint()
        {
            return new Point(_x, _y);
        }

        /// <summary>
        ///     This method must be overridden using 'public new [derived_data_type] Clone()'
        /// </summary>
        /// <returns>Clone</returns>
        public new Point Clone()
        {
            return new Point(X, Y);
        }

        /// <summary>
        ///     Vector + Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns></returns>
        public static Point operator +(Point v1, Point v2)
        {
            return new Point(v1.X + v2.X, v1.Y + v2.Y);
        }

        /// <summary>
        ///     Vector - Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns>Cross product</returns>
        public static Point operator -(Point v1, Point v2)
        {
            return new Point(v1.X - v2.X, v1.Y - v2.Y);
        }

        /// <summary>
        ///     Vector * Scalar
        /// </summary>
        /// <param name="m">Vector</param>
        /// <param name="d">Scalar (double)</param>
        /// <returns></returns>
        public static Point operator *(Point m, double d)
        {
            return new Point(m.X*d, m.Y*d);
        }

        /// <summary>
        ///     Checks whether this instance is spatially equal to the Point 'o'
        /// </summary>
        /// <param name="p">Point to compare to</param>
        /// <returns></returns>
        public virtual bool Equals(Point p)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (p != null) && (p.X == _x) && (p.Y == _y) && (_isEmpty == p.IsEmpty());
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="GetHashCode" /> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode" />.</returns>
        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _isEmpty.GetHashCode();
        }

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return _isEmpty;
        }

        /// <summary>
        ///     Returns the distance between this geometry instance and another geometry, as
        ///     measured in the spatial reference system of this instance.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override double Distance(Point point)
        {
            return Math.Sqrt(Math.Pow(X - point.X, 2) + Math.Pow(Y - point.Y, 2));
        }

        /// <summary>
        ///     Returns the distance between this point and a <see cref="Geometries.BoundingBox" />
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public double Distance(BoundingBox box)
        {
            return box.Distance(this);
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox BoundingBox => new BoundingBox(X, Y, X, Y);

        /// <summary>
        ///     Calculates a new point by rotating this point clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="centerX">X coordinate of point about which to rotate</param>
        /// <param name="centerY">Y coordinate of point about which to rotate</param>
        /// <returns>Returns the rotated point</returns>
        public Point Rotate(double degrees, double centerX, double centerY)
        {
            // translate this point back to the center
            var newX = _x - centerX;
            var newY = _y - centerY;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            newX = p.X + centerX;
            newY = p.Y + centerY;

            return new Point(newX, newY);
        }

        /// <summary>
        ///     Calculates a new point by rotating this point clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="center">Point about which to rotate</param>
        /// <returns>Returns the rotated point</returns>
        public Point Rotate(double degrees, Point center)
        {
            return Rotate(degrees, center.X, center.Y);
        }

        /// <summary>
        ///     Calculates a new point by translating this point by the specified offset
        /// </summary>
        /// <param name="offsetX">Offset to translate in X axis</param>
        /// <param name="offsetY">Offset to translate in Y axis</param>
        /// <returns>Returns the offset point</returns>
        public Point Offset(double offsetX, double offsetY)
        {
            return new Point(X + offsetX, Y + offsetY);
        }

        public override bool Contains(Point point)
        {
            return false;
        }
        
        public override bool Equals(Geometry geom)
        {
            var point = geom as Point;
            if (point == null) return false;
            return Equals(point);
        }
    }
}