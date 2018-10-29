namespace Mapsui.Geometries
{
    /// <summary>
    /// The X and Y fields of the ReadyOnlyPoint can not be set. This was introduced
    /// as Center field of the Viewport.
    /// </summary>
    public class ReadOnlyPoint
    {
        public ReadOnlyPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public ReadOnlyPoint(ReadOnlyPoint readOnlyPoint)
        {
            X = readOnlyPoint.X;
            Y = readOnlyPoint.Y;
        }

        public double X { get; }

        public double Y { get; }

        /// <summary>
        /// Implicit conversion from Point to ReadOnlyPoint
        /// </summary>
        /// <param name="readOnlyPoint"></param>
        public static implicit operator Point(ReadOnlyPoint readOnlyPoint)
        {
            return new Point(readOnlyPoint.X, readOnlyPoint.Y);
        }
        
        /// <summary>
        /// Implicit conversion from ReadOnlyPoint to Point
        /// </summary>
        /// <param name="point"></param>
        public static implicit operator ReadOnlyPoint(Point point)
        {
            return new ReadOnlyPoint(point.X, point.Y);
        }
    }
}
