using System.Collections.Generic;

namespace Mapsui.UI;

public class ViewportLimiterWithoutLimits : IViewportLimiter
{
    public MRect? PanLimits { get; set; }
    public MinMax? ZoomLimits { get; set; }

    public ViewportState Limit(ViewportState viewportState, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
        return viewportState;
    }

    public ViewportState LimitResolution(ViewportState viewportState, double screenWidth, double screenHeight, IReadOnlyList<double> mapResolutions,
        MRect? mapEnvelope)
    {
        return viewportState;
    }

    public ViewportState LimitExtent(ViewportState viewportState, MRect? mapEnvelope)
    {
        return viewportState;
    }
}
