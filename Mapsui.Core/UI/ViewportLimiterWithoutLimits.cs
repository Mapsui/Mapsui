using System.Collections.Generic;

namespace Mapsui.UI
{
    public class ViewportLimiterWithoutLimits : IViewportLimiter
    {
        public MRectangle? PanLimits { get; set; }
        public MinMax? ZoomLimits { get; set; }

        public void Limit(IViewport viewport, IReadOnlyList<double> mapResolutions, MRectangle mapEnvelope)
        {
        }

        public double LimitResolution(double resolution, double screenWidth, double screenHeight, IReadOnlyList<double> mapResolutions,
            MRectangle mapEnvelope)
        {
            return resolution;
        }

        public void LimitExtent(IViewport viewport, MRectangle? mapEnvelope)
        {
        }
    }
}
