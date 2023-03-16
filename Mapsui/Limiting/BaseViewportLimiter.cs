namespace Mapsui.Limiting;

public abstract class BaseViewportLimiter : IViewportLimiter
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

    /// <summary>
    /// Sets the limit to which the user can pan the map.
    /// If PanLimits is not set, Map.Extent will be used as restricted extent.
    /// </summary>
    public MRect? PanLimits { get; set; }

    /// <summary>
    /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
    /// to anything else than None, resolution is kept between these values.
    /// </summary>
    public MinMax? ZoomLimits { get; set; }

    public abstract ViewportState Limit(ViewportState viewportState);
}
