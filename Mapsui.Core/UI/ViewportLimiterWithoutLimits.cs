using System.Collections.Generic;

namespace Mapsui.UI
{
    public class ViewportLimiterWithoutLimits : IViewportLimiter
    {
        public MRect? PanLimits { get; set; }
        public MinMax? ZoomLimits { get; set; }

        public void Limit(IViewport viewport, IReadOnlyList<double> mapResolutions, MRect? mapEnvelope)
        {
        }

        public double LimitResolution(double resolution, double screenWidth, double screenHeight, IReadOnlyList<double> mapResolutions,
            MRect? mapEnvelope)
        {
            return resolution;
        }

        public void LimitExtent(IViewport viewport, MRect? mapEnvelope)
        {
        }
    }
}
