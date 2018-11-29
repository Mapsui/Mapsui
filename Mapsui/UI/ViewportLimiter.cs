using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

namespace Mapsui.UI
{
    public class MinMax
    {
        public MinMax(double value1, double value2)
        {
            if (value1 < value2)
            {
                Min = value1;
                Max = value2;
            }
            else
            {
                Min = value2;
                Max = value1;
            }
        }

        public double Min { get; }
        public double Max { get; }
    }

    public enum ZoomMode
    {
        /// <summary>
        /// Restricts zoom in no way
        /// </summary>
        Unlimited,
        /// <summary>
        /// Restricts zoom of the viewport to ZoomLimits and, if ZoomLimits isn't 
        /// set, to minimum and maximum of Resolutions
        /// </summary>
        KeepWithinResolutions,
    }

    public class ViewportLimiter : IViewportLimiter
    {
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

        private MinMax GetExtremes(IReadOnlyList<double> resolutions)
        {
            if (resolutions == null || resolutions.Count == 0) return null;
            resolutions = resolutions.OrderByDescending(r => r).ToList();
            var mostZoomedOut = resolutions[0];
            var mostZoomedIn = resolutions[resolutions.Count - 1] * 0.5; // divide by two to allow one extra level to zoom-in
            return new MinMax(mostZoomedOut, mostZoomedIn);
        }

        public void Limit(IViewport viewport, IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope)
        {
            viewport.SetResolution(LimitResolution(viewport.Resolution, viewport.Width, viewport.Height, mapResolutions, mapEnvelope));
            LimitExtent(viewport,  mapEnvelope);
        }

        public double LimitResolution(double resolution, double screenWidth, double screenHeight,  
            IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope)
        {
            if (ZoomMode == ZoomMode.Unlimited) return resolution;

            var resolutionExtremes = ZoomLimits ?? GetExtremes(mapResolutions);
            if (resolutionExtremes == null) return resolution;

            if (ZoomMode == ZoomMode.KeepWithinResolutions)
            {
                if (resolutionExtremes.Min > resolution) return resolutionExtremes.Min;
                if (resolutionExtremes.Max < resolution) return resolutionExtremes.Max;
            }

            return resolution;
        }
        
        public void LimitExtent(IViewport viewport, BoundingBox mapEnvelope)
        {
            var maxExtent = PanLimits ?? mapEnvelope;
            if (maxExtent == null)
            {
                // Can be null because both panLimits and Map.Extent can be null. 
                // The Map.Extent can be null if the extent of all layers is null
                return; 
            }

            var x = viewport.Center.X;
            if (viewport.Center.X < maxExtent.Left)  x = maxExtent.Left;
            if (viewport.Center.X > maxExtent.Right) x = maxExtent.Right;

            var y = viewport.Center.Y;
            if (viewport.Center.Y > maxExtent.Top) y = maxExtent.Top;
            if (viewport.Center.Y < maxExtent.Bottom) y = maxExtent.Bottom;

            viewport.SetCenter(x, y);
        }
    }
}
