namespace Mapsui
{
    /// <summary>
    /// The X and Y fields of the ReadyOnlyPoint can not be set. This was introduced
    /// as Center field of the Viewport.
    /// </summary>
    public class MReadOnlyPoint
    {
        public MReadOnlyPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public MReadOnlyPoint(MReadOnlyPoint readOnlyPoint)
        {
            X = readOnlyPoint.X;
            Y = readOnlyPoint.Y;
        }

        public double X { get; }

        public double Y { get; }

        public override string ToString() => $"(X={X},Y={Y})";

        /// <summary>
        /// Implicit conversion from MPoint to ReadOnlyPoint
        /// </summary>
        /// <param name="readOnlyPoint"></param>
        public static implicit operator MPoint(MReadOnlyPoint readOnlyPoint)
        {
            return new MPoint(readOnlyPoint.X, readOnlyPoint.Y);
        }

        /// <summary>
        /// Implicit conversion from ReadOnlyPoint to MPoint
        /// </summary>
        /// <param name="point"></param>
        public static implicit operator MReadOnlyPoint(MPoint point)
        {
            return new MReadOnlyPoint(point.X, point.Y);
        }
    }
}
