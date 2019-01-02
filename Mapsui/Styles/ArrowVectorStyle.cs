using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using System;

namespace Mapsui.Styles
{
    public class ArrowVectorStyle : VectorStyle
    {
        /// <summary>
        /// Tilt angle for the arrow branches (degree)
        /// </summary>
        public double TiltAngle { get; set; } = 45;

        /// <summary>
        /// Length of the arrow branches (px)
        /// </summary>
        public double BranchesLength { get; set; } = 10;

        /// <summary>
        /// Offset to position of the arrow branches
        /// </summary>
        public double BranchesOffset { get; set; } = 1;

        /// <summary>
        /// Compute the arrow head position
        /// </summary>
        public Point GetArrowHeadPosition(Point startPoint, Point endPoint)
        {
            return new Point(startPoint.X + (endPoint.X - startPoint.X) * BranchesOffset, startPoint.Y + (endPoint.Y - startPoint.Y) * BranchesOffset);
        }

        /// <summary>
        /// Compute the arrow branches endpoints
        /// </summary>
        public Point[] GetArrowEndPoints(LineString lineString)
        {
            var radiansAngle = Algorithms.DegreesToRadians(TiltAngle);
            var arrowHeadPosition = GetArrowHeadPosition(lineString.StartPoint, lineString.EndPoint);

            if (lineString.IsEmpty())
            {
                return new Point[] { arrowHeadPosition, arrowHeadPosition };
            }

            var tilt = lineString.StartPoint.X == lineString.EndPoint.X ?  double.PositiveInfinity : (lineString.StartPoint.Y - lineString.EndPoint.Y) / (lineString.StartPoint.X - lineString.EndPoint.X);

            var arrowOrientation = (lineString.StartPoint.X - lineString.EndPoint.X) / Math.Abs(lineString.StartPoint.X - lineString.EndPoint.X);

            var firstEndpoint = new Point(arrowHeadPosition.X + arrowOrientation * BranchesLength * Math.Cos(Math.Atan(tilt) - radiansAngle), arrowHeadPosition.Y + arrowOrientation * BranchesLength * Math.Sin(Math.Atan(tilt) - radiansAngle));
            var secondEndpoint = new Point(arrowHeadPosition.X + arrowOrientation * BranchesLength * Math.Cos(Math.Atan(tilt) + radiansAngle), arrowHeadPosition.Y + arrowOrientation * BranchesLength * Math.Sin(Math.Atan(tilt) + radiansAngle));

            return new Point[] { firstEndpoint, secondEndpoint };
        }
    }
}