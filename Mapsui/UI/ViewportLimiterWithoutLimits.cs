using System.Collections.Generic;

namespace Mapsui.UI;

public class ViewportLimiterWithoutLimits : IViewportLimiter
{
    public MRect? PanLimits { get; set; }
    public MinMax? ZoomLimits { get; set; }

    public void Limit(Viewport viewport, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
    {
    }

    public double LimitResolution(double resolution, double screenWidth, double screenHeight, IReadOnlyList<double> mapResolutions,
        MRect? mapEnvelope)
    {
        return resolution;
    }

    public void LimitExtent(Viewport viewport, MRect? mapEnvelope)
    {
    }
}
