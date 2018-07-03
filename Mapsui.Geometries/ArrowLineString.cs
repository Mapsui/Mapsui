using Mapsui.Geometries.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mapsui.Geometries
{
    /// <summary>
    ///     An ArrowLine is an oriented line with an arrow as direction indicator 
    /// </summary>
    public class ArrowLineString : MultiLineString
    {

        /// <summary>
        ///     Initializes an instance of an ArrowLine
        /// </summary>
        public ArrowLineString() : base() { }

        /// <summary>
        ///     Initializes an instance of an ArrowLine from two points, and specifications for the arrow (angle, length and position).
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="degreeAngle"></param>
        /// <param name="arrowLength"></param>
        /// <param name="arrowPositionIndicator">arrowPosition is any number between 0 and 1 to specify the position of the arrow on the line,
        ///     for instance : 0 at the Start Point, 0.5 in the middle, 1 at the End Point</param>
        public ArrowLineString(Point startPoint, Point endPoint, double degreeAngle = 45, double arrowLength = 10, double arrowPositionIndicator = 0.5) : base()
        {
            Angle = degreeAngle;
            StartPoint = startPoint;
            EndPoint = endPoint;
            ArrowLength = arrowLength;
            ArrowPositionIndicator = arrowPositionIndicator;

            LineString line = new LineString(new Point[] { startPoint, endPoint });
            LineStrings.Add(line);

            ArrowExtremities = BuildArrowExtremities(RadiansAngle, arrowLength);

            LineString firstArrowSide = new LineString(new Point[] { ArrowHead, ArrowExtremities[0] });
            LineString secondArrowSide = new LineString(new Point[] { ArrowHead, ArrowExtremities[1] });
            LineStrings.Add(firstArrowSide);
            LineStrings.Add(secondArrowSide);
        }

        /// <summary>
        ///     Initializes an instance of an ArrowLine from coordinates of two points
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>   
        public ArrowLineString(double[] startPoint, double[] endPoint, double degreeAngle = 45, double arrowLength = 10, double arrowPositionIndicator = 0.5)
            : this(new Point(startPoint), new Point(endPoint), degreeAngle, arrowLength, arrowPositionIndicator) { }

        /// <summary>
        ///    the arrow's Angle to the line (in radians)
        /// </summary>
        public double RadiansAngle => Algorithms.DegreesToRadians(Angle);

        /// <summary>
        ///    the arrow's Angle to the line (in degrees)
        /// </summary>
        public double Angle { get; }

        /// <summary>
        ///    the Point where this Geometry starts
        /// </summary>
        public Point StartPoint { get; }

        /// <summary>
        ///     the Point where this Geometry ends
        /// </summary>
        public Point EndPoint { get; }

        /// <summary>
        ///     The length of an arrow's side
        /// </summary>
        public double ArrowLength { get; }


        /// <summary>
        ///     The length of this ArrowLine, as measured in the spatial reference system of this ArrowLine.
        /// </summary>
        private double? length;
        public override double Length => length.HasValue ? length.Value : (length = StartPoint.Distance(EndPoint)).Value;

        /// <summary>
        /// The indicator of the arrow's position on the line, double value between 0 and 1.
        /// We get 0 if the arrow is at the start point, 0.5 if it is in the middle, 1 if it is at the end point etc.
        /// </summary>
        public double ArrowPositionIndicator { get; }

        /// <summary>
        ///     The position of the arrow on the line, placed following the ArrowPositionIndicator
        /// </summary>
        private Point arrowHead;
        public Point ArrowHead => arrowHead ?? (arrowHead = new Point(StartPoint.X + (EndPoint.X - StartPoint.X) * ArrowPositionIndicator, StartPoint.Y + (EndPoint.Y - StartPoint.Y) * ArrowPositionIndicator));


        /// <summary>
        ///     Build the two points linked to the 'ArrowHead' in order to draw the sides of the arrow
        /// </summary>
        /// <param name="radiansAngle"></param>   
        /// <param name="length"></param>   
        /// <returns>Returns a set of two Points</returns>
        public Point[] BuildArrowExtremities(double radiansAngle, double length)
        {
            if (IsEmpty())
                return new Point[] { ArrowHead, ArrowHead };

            double slope;
            if (StartPoint.X == EndPoint.X)  //Prevents div/0
                slope = double.PositiveInfinity;
            else
                slope = (StartPoint.Y - EndPoint.Y) / (StartPoint.X - EndPoint.X);

            double arrowOrientation = (StartPoint.X - EndPoint.X) / Math.Abs(StartPoint.X - EndPoint.X);

            Point firstExt = new Point(ArrowHead.X + arrowOrientation * length * Math.Cos(Math.Atan(slope) - radiansAngle), ArrowHead.Y + arrowOrientation * length * Math.Sin(Math.Atan(slope) - radiansAngle));
            Point secondExt = new Point(ArrowHead.X + arrowOrientation * length * Math.Cos(Math.Atan(slope) + radiansAngle), ArrowHead.Y + arrowOrientation * length * Math.Sin(Math.Atan(slope) + radiansAngle));

            return new Point[] { firstExt, secondExt };
        }

        /// <summary>
        ///     The two points linked to the 'ArrowHead' in order to draw the sides of the arrow
        /// </summary>
        public Point[] ArrowExtremities { get; }

        /// <summary>
        ///     The minimum bounding box for this Geometry.
        /// </summary>
        /// <returns>BoundingBox for this geometry</returns>
        public override BoundingBox BoundingBox
        {
            get
            {
                Point[] pointsCollection = new Point[] { StartPoint, EndPoint, ArrowExtremities[0], ArrowExtremities[1] };
                var bbox = new BoundingBox()
                {
                    Min = new Point(pointsCollection.Min(point => point.X), pointsCollection.Min(point => point.Y)),
                    Max = new Point(pointsCollection.Max(point => point.X), pointsCollection.Max(point => point.Y))
                };
                return bbox;
            }
        }

        /// <summary>
        ///     Serves as a hash function for a particular type. <see cref="GetHashCode" /> is suitable for use
        ///     in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>A hash code for the current <see cref="GetHashCode" />.</returns>
        public override int GetHashCode()
        {
            return LineStrings.Aggregate(0, (current, t) => current ^ t.GetHashCode());
        }

        public override bool Equals(Geometry geom)
        {
            var arrowLine = geom as ArrowLineString;
            if (arrowLine == null) return false;
            return Equals(arrowLine);
        }
    }
}

