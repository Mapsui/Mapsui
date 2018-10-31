using System.ComponentModel;
using Mapsui.Geometries;

namespace Mapsui
{
    public interface IReadOnlyViewport 
    {
        event PropertyChangedEventHandler ViewportChanged;

        /// <summary>
        /// Coordinate of center of viewport in map coordinates
        /// </summary>
        ReadOnlyPoint Center { get; } // todo: the point itself has X and Y values that can be set. 

        /// <summary>
        /// Resolution of the viewport in units per pixel
        /// </summary>
        /// <remarks>
        /// Resolution is Mapsuis form of zoom level. Because Mapsui is projection independent, there 
        /// aren't any zoom levels as other map libraries have. If your map has EPSG:3857 as projection
        /// and you want to calculate the zoom, you should use the following equation
        /// 
        ///     var zoom = (float)Math.Log(78271.51696401953125 / resolution, 2);
        /// </remarks>
        double Resolution { get; }

        /// <summary>
        /// BoundingBox of viewport in map coordinates respection Rotation
        /// </summary>
        /// <remarks>
        /// This BoundingBox is horizontally and vertically aligned, even if the viewport
        /// is rotated. So this BoundingBox perhaps contain parts, that are not visible.
        /// </remarks>
        BoundingBox Extent { get; }

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
        /// <returns>Point in map units</returns>
        Point ScreenToWorld(Point position);

        /// <summary>
        /// Converts X/Y in screen pixels to a point in map units, respecting rotation
        /// </summary>
        /// <param name="x">Screen position x coordinate</param>
        /// <param name="y">Screen position y coordinate</param>
        /// <returns>Point in map units</returns>
        Point ScreenToWorld(double x, double y);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent unit (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldPosition">Coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreen(Point worldPosition);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldX">X coordinate in map units</param>
        /// <param name="worldY">Y coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreen(double worldX, double worldY);

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldX">X coordinate in map units</param>
        /// <param name="worldY">Y coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(double worldX, double worldY); // todo: Get rid of this method

        /// <summary>
        /// Converts X/Y in map units to a point in device independent units (or DIP or DP),
        /// respecting rotation
        /// </summary>
        /// <param name="worldPosition">Coordinate in map units</param>
        /// <returns>Point in screen pixels</returns>
        Point WorldToScreenUnrotated(Point worldPosition); // todo: Get rid of this method

        /// <summary>
        /// WindowExtend gives the four corner points of viewport in map coordinates
        /// </summary>
        /// <remarks>
        /// If viewport is rotated, this corner points are not horizontally or vertically
        /// aligned.
        /// </remarks>
        Quad WindowExtent { get; }
    }
}
