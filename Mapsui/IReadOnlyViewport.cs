using System.ComponentModel;

namespace Mapsui;

public interface IReadOnlyViewport
{
    event PropertyChangedEventHandler ViewportChanged;

    /// <summary>
    /// MRect of viewport in world coordinates respecting Rotation
    /// </summary>
    /// <remarks>
    /// This MRect is horizontally and vertically aligned, even if the viewport
    /// is rotated. So this MRect perhaps contain parts, that are not visible.
    /// </remarks>
    MRect? Extent { get; }

    ViewportState State { get; }

    /// <summary>
    /// Converts a point in screen pixels to one in screen units, respecting rotation
    /// </summary>
    /// <param name="position">Coordinate in screen units</param>
    /// <returns>MPoint in world units</returns>
    MPoint ScreenToWorld(MPoint position);

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>MPoint in world units</returns>
    MPoint ScreenToWorld(double x, double y);

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>Tuple of x and y in world coordinates</returns>
    (double worldX, double worldY) ScreenToWorldXY(double x, double y);

    /// <summary>
    /// Converts X/Y in world units to a point in device independent unit (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldPosition">Coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>
    MPoint WorldToScreen(MPoint worldPosition);

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>
    MPoint WorldToScreen(double worldX, double worldY);

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>Tuple of x and y in screen coordinates</returns>
    (double screenX, double screenY) WorldToScreenXY(double worldX, double worldY);
}
