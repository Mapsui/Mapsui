using System.Collections.Generic;

namespace Mapsui.UI;

public interface IViewportLimiter
{
    /// <summary>
    /// Sets the limit to which the user can pan the map.
    /// If PanLimits is not set, Map.Extent will be used as restricted extent.
    /// </summary>
    MRect? PanLimits { get; set; }

    /// <summary>
    /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
    /// to anything else than None, resolution is kept between these values.
    /// </summary>
    MinMax? ZoomLimits { get; set; }

    void Limit(Viewport viewport, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope);

    double LimitResolution(double resolution, double screenWidth, double screenHeight,
        IReadOnlyList<double> mapResolutions, MRect? mapEnvelope);

    void LimitExtent(Viewport viewport, MRect? mapEnvelope);
}
