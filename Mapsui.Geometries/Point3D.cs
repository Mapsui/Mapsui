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

namespace Mapsui.Geometries
{
    /// <summary>
    /// A Point3D is a 0-dimensional geometry and represents a single location in 3D coordinate space. A Point3D has a x coordinate
    /// value, a y-coordinate value and a z-coordinate value. The boundary of a Point3D is the empty set.
    /// </summary>
    public class Point3D : Point
    {
        private double z;

        /// <summary>
        /// Initializes a new Point
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public Point3D(double x, double y, double z) : base(x, y)
        {
            this.z = z;
        }

        /// <summary>
        /// Initializes a new Point
        /// </summary>
        /// <param name="p">2D Point</param>
        /// <param name="z">Z coordinate</param>
        public Point3D(Point p, double z)
            : base(p.X, p.Y)
        {
            this.z = z;
        }

        /// <summary>
        /// Initializes a new Point at (0,0)
        /// </summary>
        public Point3D() : this(0, 0, 0)
        {
            SetIsEmpty = true;
        }

        /// <summary>
        /// Create a new point by a douuble[] array
        /// </summary>
        /// <param name="point"></param>
        public Point3D(double[] point)
            : base(point[0], point[1])
        {
            if (point.Length != 3)
                throw new Exception("Only 3 dimensions are supported for points");

            z = point[2];
        }

        /// <summary>
        /// Gets or sets the Z coordinate of the point
        /// </summary>
        public double Z
        {
            get
            {
                if (!IsEmpty())
                    return z;
                throw new Exception("Point is empty");
            }
            set
            {
                z = value;
                SetIsEmpty = false;
            }
        }

        /// <summary>
        /// Returns part of coordinate. Index 0 = X, Index 1 = Y, , Index 2 = Z
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override double this[uint index]
        {
            get
            {
                if (index == 2)
                {
                    if (IsEmpty())
                        throw new Exception("Point is empty");
                    return Z;
                }
                return base[index];
            }
            set
            {
                if (index == 2)
                {
                    Z = value;
                    SetIsEmpty = false;
                }
                else base[index] = value;
            }
        }

        /// <summary>
        /// Returns the number of ordinates for this point
        /// </summary>
        public override int NumOrdinates
        {
            get { return 3; }
        }

        
        /// <summary>
        /// Vector + Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns></returns>
        public static Point3D operator +(Point3D v1, Point3D v2)
        {
            return new Point3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }


        /// <summary>
        /// Vector - Vector
        /// </summary>
        /// <param name="v1">Vector</param>
        /// <param name="v2">Vector</param>
        /// <returns>Cross product</returns>
        public static Point3D operator -(Point3D v1, Point3D v2)
        {
            return new Point3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        /// <summary>
        /// Vector * Scalar
        /// </summary>
        /// <param name="m">Vector</param>
        /// <param name="d">Scalar (double)</param>
        /// <returns></returns>
        public static Point3D operator *(Point3D m, double d)
        {
            return new Point3D(m.X*d, m.Y*d, m.Z*d);
        }

        
        
        /// <summary>
        /// Checks whether this instance is spatially equal to the Point 'o'
        /// </summary>
        /// <param name="p">Point to compare to</param>
        /// <returns></returns>
        public bool Equals(Point3D p)
        {
            return base.Equals(p) && p.Z == z;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="GetHashCode"/> is suitable for use 
        /// in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode"/>.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ z.GetHashCode();
        }

        /// <summary>
        /// Returns the distance between this geometry instance and another geometry, as
        /// measured in the spatial reference system of this instance.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public override double Distance(Geometry geom)
        {
            if (geom.GetType() == typeof (Point3D))
            {
                var p = geom as Point3D;
                return Math.Sqrt(Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2) + Math.Pow(Z - p.Z, 2));
            }
            return base.Distance(geom);
        }

        
        /// <summary>
        /// exports a point into a 3-dimensional double array
        /// </summary>
        /// <returns></returns>
        public new double[] ToDoubleArray()
        {
            return new[] {X, Y, Z};
        }

        /// <summary>
        /// This method must be overridden using 'public new [derived_data_type] Clone()'
        /// </summary>
        /// <returns>Clone</returns>
        public new Point3D Clone()
        {
            return new Point3D(X, Y, Z);
        }

        /// <summary>
        /// Checks whether the two points are spatially equal
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>true if the points a spatially equal</returns>
        public bool Equals(Point3D p1, Point3D p2)
        {
            return (p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z);
        }

        /// <summary>
        /// Comparator used for ordering point first by ascending X, then by ascending Y and then by ascending Z.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(Point3D other)
        {
            if (X < other.X || X == other.X && Y < other.Y || X == other.X && Y == other.Y && Z < other.Z)
                return -1;

            if (X > other.X || X == other.X && Y > other.Y || X == other.X && Y == other.Y && Z > other.Z)
                return 1;

            return 0;
        }
    }
}