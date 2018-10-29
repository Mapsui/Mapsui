using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.UI
{
    public interface IViewportLimiter
    {
        /// <summary>
        /// Set this property in combination KeepCenterWithinExtents or KeepViewportWithinExtents.
        /// If PanLimits is not set, Map.Extent will be used as restricted extent.
        /// </summary>
        BoundingBox PanLimits { get; set; }

        /// <summary>
        /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
        /// to anything else than None, resolution is kept between these values.
        /// </summary>
        MinMax ZoomLimits { get; set; }

        void Limit(IViewport viewport, IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope);

        double LimitResolution(double resolution, double screenWidth, double screenHeight, 
            IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope);

        void LimitExtent(IViewport viewport, BoundingBox mapEnvelope);
    }
}