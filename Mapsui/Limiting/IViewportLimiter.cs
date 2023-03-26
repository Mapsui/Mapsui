namespace Mapsui.Limiting;

public interface IViewportLimiter
{
    /// <summary>
    /// When true the user can not pan (move) the map.
    /// </summary>
    public bool PanLock { get; set; }

    /// <summary>
    /// When true the user an not rotate the map
    /// </summary>
    public bool ZoomLock { get; set; }

    /// <summary>
    /// When true the user can not zoom into the map
    /// </summary>
    public bool RotationLock { get; set; }
    Viewport Limit(Viewport viewport, MRect? panExtent, MMinMax? zoomExtremes);
}
