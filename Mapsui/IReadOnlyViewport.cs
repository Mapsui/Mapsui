using Mapsui.Geometries;

namespace Mapsui
{
    public interface IReadOnlyViewport 
    {
        Point Center { get; }
        double Resolution { get; set; }
        BoundingBox Extent { get; }
        double Width { get; }
        double Height { get; }
        double Rotation { get; }
        bool Initialized { get; }
        bool IsRotated { get; }
        Point ScreenToWorld(Point screenPosition);
        Point ScreenToWorld(double x, double y);
        Point WorldToScreen(Point worldPosition);
        Point WorldToScreen(double worldX, double worldY);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="x">X coordinate in map units</param>
        /// <param name="y">Y coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(double x, double y); // todo: Get rid of this method

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="point">Coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(Point point); // todo: Get rid of this method
        void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY,
            double deltaScale = 1, double deltaRotation = 0);
    }
}
