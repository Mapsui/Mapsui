using System;
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

    public enum LimitExtentMode
    {
        None,
        /// <summary>
        /// Restricts center of the viewport within Map.Extents or within MaxExtents when set
        /// </summary>
        KeepCenterWithinExtents,
        /// <summary>
        /// Restricts the whole viewport within Map.Extents or within MaxExtents when set
        /// </summary>
        KeepViewportWithinExtents,
    }

    public enum LimitResolutionMode
    {
        None,
        KeepWithinResolutions,
        KeepWithinResolutionsAndAlwaysFillViewport
    }

    public class ViewportLimiter
    {
        public LimitExtentMode PanMode { get; set; } = LimitExtentMode.KeepViewportWithinExtents;

        public LimitResolutionMode ZoomMode { get; set; } = LimitResolutionMode.KeepWithinResolutionsAndAlwaysFillViewport;

        /// <summary>
        /// Set this property in combination KeepCenterWithinExtents or KeepViewportWithinExtents.
        /// If ExtentLimits is not set Map.Extent will be used as restricted extent.
        /// </summary>
        public BoundingBox ExtentLimits { get; set; }

        /// <summary>
        /// Pair of the extreme resolution (biggest and smalles). The resolution is kept between these.
        /// The order of the two extreme resolutions does not matter.
        /// </summary>
        public MinMax ResolutionLimits { get; set; }

        private static MinMax GetExtremes(IList<double> resolutions)
        {
            if (resolutions == null || resolutions.Count == 0) return null;
            resolutions = resolutions.OrderByDescending(r => r).ToList();
            var mostZoomedOut = resolutions[0];
            var mostZoomedIn = resolutions[resolutions.Count - 1]/2; // divide by two to allow one extra level zoom-in
            return new MinMax(mostZoomedOut, mostZoomedIn);
        }

        public void Limit(IViewport viewport, IList<double> resolutions, BoundingBox mapEnvelope)
        {
            LimitResolution(viewport, resolutions, mapEnvelope);
            LimitEnvelope(viewport, mapEnvelope);
        }

        public void LimitResolution(IViewport viewport, IList<double> resolutions, BoundingBox mapEnvelope)
        {
            if (ZoomMode == LimitResolutionMode.None) return;

            var resolutionExtremes = ResolutionLimits ?? GetExtremes(resolutions);
            if (resolutionExtremes == null) return;

            if (ZoomMode == LimitResolutionMode.KeepWithinResolutions)
            {
                if (resolutionExtremes.Min > viewport.Resolution) viewport.Resolution = resolutionExtremes.Min;
                if (resolutionExtremes.Max < viewport.Resolution) viewport.Resolution = resolutionExtremes.Max;
            }
            else if (ZoomMode == LimitResolutionMode.KeepWithinResolutionsAndAlwaysFillViewport)
            {
                if (resolutionExtremes.Min > viewport.Resolution) viewport.Resolution = resolutionExtremes.Min;
                
                // This is the ...AndAlwaysFillViewport part
                var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewport, mapEnvelope);
                if (viewportFillingResolution < resolutionExtremes.Min) viewport.Resolution = viewport.Resolution; // Mission impossible. Can't adhere to both restrictions
                var limit = Math.Min(resolutionExtremes.Max, viewportFillingResolution);
                if (limit < viewport.Resolution) viewport.Resolution = limit;
            }
        }

        private double CalculateResolutionAtWhichMapFillsViewport(IViewport viewport, BoundingBox mapEnvelope)
        {
            return Math.Min(mapEnvelope.Width / viewport.Width, mapEnvelope.Height / viewport.Height);
        }

        public void LimitEnvelope(IViewport viewport, BoundingBox mapEnvelope)
        {
            var maxExtent = ExtentLimits ?? mapEnvelope;
            if (maxExtent == null) return; // Can be null because both ExtentLimits and Map.Extent can be null. The Map.Extent can be null if the extent of all layers is null

            if (PanMode == LimitExtentMode.KeepCenterWithinExtents)
            {
                if (viewport.Center.X < maxExtent.Left) viewport.Center.X = maxExtent.Left;
                if (viewport.Center.X > maxExtent.Right) viewport.Center.X = maxExtent.Right;
                if (viewport.Center.Y > maxExtent.Top) viewport.Center.Y = maxExtent.Top;
                if (viewport.Center.Y < maxExtent.Bottom) viewport.Center.Y = maxExtent.Bottom;
            }
            else if (PanMode == LimitExtentMode.KeepViewportWithinExtents)
            {
                if (MapWidthSpansViewport(maxExtent.Width, viewport.Width, viewport.Resolution)) // if it does't fit don't restrict
                {
                    if (viewport.Extent.Left < maxExtent.Left)
                        viewport.Center.X += maxExtent.Left - viewport.Extent.Left;
                    if (viewport.Extent.Right > maxExtent.Right)
                        viewport.Center.X += maxExtent.Right - viewport.Extent.Right;
                }
                if (MapHeightSpansViewport(maxExtent.Height, viewport.Height, viewport.Resolution)) // if it does't fit don't restrict
                {
                    if (viewport.Extent.Top > maxExtent.Top)
                        viewport.Center.Y += maxExtent.Top - viewport.Extent.Top;
                    if (viewport.Extent.Bottom < maxExtent.Bottom)
                        viewport.Center.Y += maxExtent.Bottom - viewport.Extent.Bottom;
                }
            }
        }

        private static bool MapWidthSpansViewport(double extentWidth, double viewportWidth, double resolution)
        {
            var mapWidth = extentWidth / resolution; // in screen units
            return viewportWidth <= mapWidth;
        }

        private static bool MapHeightSpansViewport(double extentHeight, double viewportHeight, double resolution)
        {
            var mapHeight = extentHeight / resolution; // in screen units
            return viewportHeight <= mapHeight;
        }
    }
}
