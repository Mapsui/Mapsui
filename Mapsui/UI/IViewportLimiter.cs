using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui
{
    public interface IViewportLimiter
    {
        /// <summary>
        /// Pan mode to use, when map is paned
        /// </summary>
        PanMode PanMode { get; set; }

        /// <summary>
        /// Zoom mode to use, when map is zoomed
        /// </summary>
        ZoomMode ZoomMode { get; set; }

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