namespace Mapsui;

/// <summary>
/// Specifies the coordinate space of an <see cref="MRect"/> passed to
/// <see cref="Map.RefreshGraphics(MRect, CoordinateSpace)"/>.
/// </summary>
public enum CoordinateSpace
{
    /// <summary>
    /// World/map coordinates (e.g. EPSG:3857 metres).
    /// Use for data-driven updates such as moving a GPS marker.
    /// </summary>
    World,

    /// <summary>
    /// Screen coordinates in device-independent pixels.
    /// Use for widget-area updates that are fixed to the screen, such as
    /// refreshing the area occupied by a custom widget.
    /// </summary>
    Screen,
}
