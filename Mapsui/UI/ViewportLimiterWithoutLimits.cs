using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.UI
{
    public class ViewportLimiterWithoutLimits : IViewportLimiter
    {
        public BoundingBox PanLimits { get; set; }
        public MinMax ZoomLimits { get; set; }
        public void Limit(IViewport viewport, IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope)
        {
        }

        public double LimitResolution(double resolution, double screenWidth, double screenHeight, IReadOnlyList<double> mapResolutions,
            BoundingBox mapEnvelope)
        {
            return resolution;
        }

        public void LimitExtent(IViewport viewport, BoundingBox mapEnvelope)
        {
        }
    }
}
