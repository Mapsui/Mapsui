namespace Mapsui;

public interface IViewportState
{
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
    /// MRect of viewport in world coordinates respecting Rotation
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
}
