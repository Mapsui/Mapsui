using System;
using Mapsui.Geometries.Utilities;

// ReSharper disable NonReadonlyMemberInGetHashCode // todo: Fix this real issue
namespace Mapsui.Geometries
{
    /// <summary>
    ///     A Circle is a geometry that represents a single location in 2D coordinate space and the area around it with radius. A Circle has a x
    ///     coordinate value and a y-coordinate value. The boundary of a Point is the empty set.
    /// </summary>
    public class Circle : Geometry, IComparable<Circle>
    {
        private bool _isEmpty;
        private double _x;
        private double _y;
        private double _radius;

        /// <summary>
        ///     Initializes a new Point
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public Circle(double x, double y, double radius)
        {
            _x = x;
            _y = y;
            _radius = radius;
        }

        /// <summary>
        ///     Initializes a new empty Point
        /// </summary>
        public Circle() : this(0, 0, 0)
        {
            _isEmpty = true;
        }

        /// <summary>
        ///     Create a new point by a double[] array
        /// </summary>
        /// <param name="point"></param>
        public Circle(double[] point, double radius)
        {
            if (point.Length != 2)
                throw new Exception("Only 2 dimensions are supported for points");

            _x = point[0];
            _y = point[1];
            _radius = radius;
        }

        /// <summary>
        ///     Sets whether this object is empty
        /// </summary>
        protected bool SetIsEmpty
        {
            set { _isEmpty = value; }
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
        ///     Gets or sets the Radius of the circle measured in the spatial reference system of this instance
        /// </summary>
        public double Radius
        {
            get
            {
                if (!_isEmpty)
                    return _radius;
                throw new Exception("Point is empty");
            }
            set
            {
                _radius = value;
                _isEmpty = false;
            }
        }

        /// <summary>
        ///     Returns part of coordinate. Index 0 = X, Index 1 = Y, Index 2 = Radius
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
                if (index == 1)
                    return Y;
                if (index == 2)
                    return Radius;
                throw new Exception("Point index out of bounds");
            }
            set
            {
                if (index == 0)
                    X = value;
                else if (index == 1)
                    Y = value;
                else if (index == 2)
                    Radius = value;
                else
                    throw new Exception("Point index out of bounds");
                _isEmpty = false;
            }
        }

        /// <summary>
        ///     Returns the number of ordinates for this point
        /// </summary>
        public virtual int NumOrdinates
        {
            get { return 2; }
        }

        /// <summary>
        ///     Comparator used for ordering point first by ascending X, then by ascending Y, then ascending Radius.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(Circle other)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((X < other.X) || ((X == other.X) && (Y < other.Y)) || ((X == other.X) && (Y == other.Y) && (Radius < other.Radius)))
                return -1;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((X > other.X) || ((X == other.X) && (Y > other.Y)) || ((X == other.X) && (Y == other.Y) && (Radius > other.Radius)))
                return 1;

            return 0;
        }

        /// <summary>
        ///     This method must be overridden using 'public new [derived_data_type] Clone()'
        /// </summary>
        /// <returns>Clone</returns>
        public new Circle Clone()
        {
            return new Circle(X, Y, Radius);
        }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns>BoundingBox for this geometry</returns>
        public override BoundingBox GetBoundingBox()
        {
            if (_isEmpty)
                return null;

            var bbox = new BoundingBox(new Point(X - Radius, Y - Radius), new Point(X + Radius, Y + Radius));
            
            return bbox;
        }

        /// <summary>
        ///     Checks whether this instance is spatially equal to the Circle 'o'
        /// </summary>
        /// <param name="c">Circle to compare to</param>
        /// <returns></returns>
        public virtual bool Equals(Circle c)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (c != null) && (c.X == _x) && (c.Y == _y) && (c.Radius == _radius) && (_isEmpty == c.IsEmpty());
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


        public override bool Equals(Geometry geom)
        {
            if (geom is Circle)
                return false;
            else
                return Equals(geom as Circle);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="GetHashCode" /> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode" />.</returns>
        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _radius.GetHashCode() ^ _isEmpty.GetHashCode();
        }

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            return _isEmpty;
        }

        public override double Distance(Point point)
        {
            return point.Distance(new Point(_x, _y)) - _radius < 0 ? 0 : point.Distance(new Point(_x, _y)) - _radius;
        }

        /// <summary>
        ///     Returns the distance between this circle and a <see cref="BoundingBox" />
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public double Distance(BoundingBox box)
        {
            return box.Distance(new Point(_x, _y)) - _radius < 0 ? 0 : box.Distance(new Point(_x, _y)) - _radius;
        }

        /// <summary>
        ///     Calculates a new circle by rotating the center clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="centerX">X coordinate of point about which to rotate</param>
        /// <param name="centerY">Y coordinate of point about which to rotate</param>
        /// <returns>Returns the rotated circle</returns>
        public Circle Rotate(double degrees, double centerX, double centerY)
        {
            // translate this point back to the center
            var newX = _x - centerX;
            var newY = _y - centerY;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            newX = p.X + centerX;
            newY = p.Y + centerY;

            return new Circle(newX, newY, _radius);
        }

        /// <summary>
        ///     Calculates a new circle by rotating this point clockwise about the specified center point
        /// </summary>
        /// <param name="degrees">Angle to rotate clockwise (degrees)</param>
        /// <param name="center">Point about which to rotate</param>
        /// <returns>Returns the rotated point</returns>
        public Circle Rotate(double degrees, Point center)
        {
            return Rotate(degrees, center.X, center.Y);
        }

        /// <summary>
        ///     Calculates a new circle by translating this point by the specified offset
        /// </summary>
        /// <param name="offsetX">Offset to translate in X axis</param>
        /// <param name="offsetY">Offset to translate in Y axis</param>
        /// <returns>Returns the offset point</returns>
        public Circle Offset(double offsetX, double offsetY)
        {
            return new Circle(X + offsetX, Y + offsetY, _radius);
        }

        public override bool Contains(Point point)
        {
            return point.Distance(new Point(_x, _y)) <= _radius;
        }
    }
}