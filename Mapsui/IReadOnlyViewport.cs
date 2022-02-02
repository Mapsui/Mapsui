using System.ComponentModel;

namespace Mapsui
{
    public interface IReadOnlyViewport
    {
        event PropertyChangedEventHandler ViewportChanged;

        /// <summary>
        /// Coordinate of center of viewport in map coordinates
        /// </summary>
        MReadOnlyPoint Center { get; }

        double CenterX { get; }
        double CenterY { get; }

        /// <summary>
        /// Resolution of the viewport in units per pixel
        /// </summary>
        /// <remarks>
        /// The Resolution in Mapsui is what is often called zoom level. Because Mapsui is projection independent, there 
        /// aren't any zoom levels as other map libraries have. If your map has EPSG:3857 as projection
        /// and you want to calculate the zoom, you should use the following equation
        /// 
        ///     var zoom = (float)Math.Log(78271.51696401953125 / resolution, 2);
        /// </remarks>
        double Resolution { get; }

        /// <summary>
        /// MRect of viewport in map coordinates respecting Rotation
        /// </summary>
        /// <remarks>
        /// This MRect is horizontally and vertically aligned, even if the viewport
        /// is rotated. So this MRect perhaps contain parts, that are not visible.
        /// </remarks>
        MRect? Extent { get; }

        /// <summary>
        /// Width of viewport in screen pixels
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Height of viewport in screen pixels
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Viewport rotation from True North (clockwise degrees)
        /// </summary>
        double Rotation { get; }

        bool HasSize { get; }

        /// <summary>
        /// IsRotated is true, when viewport displays map rotated
        /// </summary>
        bool IsRotated { get; }

        /// <summary>
        /// Converts a point in screen pixels to one in map units, respecting rotation
        /// </summary>
        /// <param name="position">Coordinate in map units</param>
        /// <returns>MPoint in map units</returns>
        MPoint ScreenToWorld(MPoint position);

        /// <summary>
        /// Converts X/Y in screen pixels to a point in map units, respecting rotation
        /// </summary>
        /// <param name="x">Screen position x coordinate</param>
        /// <param name="y">Screen position y coordinate</param>
        /// <returns>MPoint in map units</returns>
        MPoint ScreenToWorld(double x, double y);

        /// <summary>
        /// Converts X/Y in screen pixels to a point in map units, respecting rotation
        /// </summary>
        /// <param name="x">Screen position x coordinate</param>
        /// <param name="y">Screen position y coordinate</param>
        /// <returns>Tuple of x and y in world coordintes</returns>
        (double worldX, double worldY) ScreenToWorldXY(double x, double y);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent unit (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldPosition">Coordinate in map units</param>
        /// <returns>MPoint in screen pixels</returns>
        MPoint WorldToScreen(MPoint worldPosition);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldX">X coordinate in map units</param>
        /// <param name="worldY">Y coordinate in map units</param>
        /// <returns>MPoint in screen pixels</returns>
        MPoint WorldToScreen(double worldX, double worldY);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldX">X coordinate in map units</param>
        /// <param name="worldY">Y coordinate in map units</param>
        /// <returns>Tuple of x and y in screen coordinates</returns>
        (double screenX, double screenY) WorldToScreenXY(double worldX, double worldY);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldX">X coordinate in map units</param>
        /// <param name="worldY">Y coordinate in map units</param>
        /// <returns>The x and y in screen pixels</returns>
        (double screenX, double screenY) WorldToScreenUnrotated(double worldX, double worldY); // todo: Get rid of this method
    }
}
