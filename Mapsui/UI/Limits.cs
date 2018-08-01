using Mapsui.Geometries;

namespace Mapsui.UI
{
    public class Limits : ILimits
    {
        /// <summary>
        /// Pan mode to use, when map is paned
        /// </summary>
        public PanMode PanMode { get; set; } = PanMode.KeepCenterWithinExtents;

        /// <summary>
        /// Zoom mode to use, when map is zoomed
        /// </summary>
        public ZoomMode ZoomMode { get; set; } = ZoomMode.KeepWithinResolutions;

        /// <summary>
        /// Set this property in combination KeepCenterWithinExtents or KeepViewportWithinExtents.
        /// If PanLimits is not set, Map.Extent will be used as restricted extent.
        /// </summary>
        public BoundingBox PanLimits { get; set; }

        /// <summary>
        /// Pair of the limits for the resolutions (smallest and biggest). If ZoomMode is set 
        /// to anything else than None, resolution is kept between these values.
        /// </summary>
        public MinMax ZoomLimits { get; set; }
    }
}
