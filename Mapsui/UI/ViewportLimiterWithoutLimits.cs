using System.Collections.Generic;

namespace Mapsui.UI;

public class ViewportLimiterWithoutLimits : IViewportLimiter
{
    public MRect? PanLimits { get; set; }
    public MinMax? ZoomLimits { get; set; }

    public ViewportState Limit(ViewportState viewportState, IReadOnlyList<double>? mapResolutions, MRect? mapEnvelope)
    {
        return viewportState;
    }
}
