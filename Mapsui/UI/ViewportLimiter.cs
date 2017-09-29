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

    public enum PanMode
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

    public enum ZoomMode
    {
        None,
        KeepWithinResolutions,
        KeepWithinResolutionsAndAlwaysFillViewport
    }

    public static class ViewportLimiter
    {
        public static MinMax GetExtremes(IReadOnlyList<double> resolutions)
        {
            if (resolutions == null || resolutions.Count == 0) return null;
            resolutions = resolutions.OrderByDescending(r => r).ToList();
            var mostZoomedOut = resolutions[0];
            var mostZoomedIn = resolutions[resolutions.Count - 1] * 0.5; // divide by two to allow one extra level to zoom-in
            return new MinMax(mostZoomedOut, mostZoomedIn);
        }

        public static void Limit(IViewport viewport, 
            ZoomMode zoomMode, MinMax zoomLimits, IReadOnlyList<double> mapResolutions, 
            PanMode panMode, BoundingBox panExtents, BoundingBox mapEnvelope)
        {
            LimitResolution(viewport, zoomMode, zoomLimits, mapResolutions, mapEnvelope);
            LimitExtent(viewport, panMode, panExtents, mapEnvelope);
        }

        public static void LimitResolution(IViewport viewport, ZoomMode zoomMode, MinMax zoomLimits, 
            IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope)
        {
            if (zoomMode == ZoomMode.None) return;

            var resolutionExtremes = zoomLimits ?? GetExtremes(mapResolutions);
            if (resolutionExtremes == null) return;

            if (zoomMode == ZoomMode.KeepWithinResolutions)
            {
                if (resolutionExtremes.Min > viewport.Resolution) viewport.Resolution = resolutionExtremes.Min;
                if (resolutionExtremes.Max < viewport.Resolution) viewport.Resolution = resolutionExtremes.Max;
            }
            else if (zoomMode == ZoomMode.KeepWithinResolutionsAndAlwaysFillViewport)
            {
                if (resolutionExtremes.Min > viewport.Resolution) viewport.Resolution = resolutionExtremes.Min;
                
                // This is the ...AndAlwaysFillViewport part
                var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(viewport, mapEnvelope);
                if (viewportFillingResolution < resolutionExtremes.Min) viewport.Resolution = viewport.Resolution; // Mission impossible. Can't adhere to both restrictions
                var limit = Math.Min(resolutionExtremes.Max, viewportFillingResolution);
                if (limit < viewport.Resolution) viewport.Resolution = limit;
            }
        }

        private static double CalculateResolutionAtWhichMapFillsViewport(IViewport viewport, BoundingBox mapEnvelope)
        {
            return Math.Min(mapEnvelope.Width / viewport.Width, mapEnvelope.Height / viewport.Height);
        }

        public static void LimitExtent(IViewport viewport, PanMode panMode, BoundingBox panLimits, BoundingBox mapEnvelope)
        {
            var maxExtent = panLimits ?? mapEnvelope;
            if (maxExtent == null) return; // Can be null because both panLimits and Map.Extent can be null. The Map.Extent can be null if the extent of all layers is null

            if (panMode == PanMode.KeepCenterWithinExtents)
            {
                if (viewport.Center.X < maxExtent.Left) viewport.Center.X = maxExtent.Left;
                if (viewport.Center.X > maxExtent.Right) viewport.Center.X = maxExtent.Right;
                if (viewport.Center.Y > maxExtent.Top) viewport.Center.Y = maxExtent.Top;
                if (viewport.Center.Y < maxExtent.Bottom) viewport.Center.Y = maxExtent.Bottom;
            }
            else if (panMode == PanMode.KeepViewportWithinExtents)
            {
                var cos = Math.Abs(Math.Cos(viewport.Rotation * Utilities.Constants.DegreesToRadians));
                var sin = Math.Abs(Math.Sin(viewport.Rotation * Utilities.Constants.DegreesToRadians));

                var window = viewport.WindowExtent.ToBoundingBox();

                if (MapWidthSpansViewport(maxExtent.Width, viewport.Width, viewport.Resolution)) // if it does't fit don't restrict
                {
                    //if ((viewport.Extent.Left < maxExtent.Left) && (viewport.Extent.Right > maxExtent.Right))
                    //    throw new Exception();
                    if (viewport.Extent.Left < maxExtent.Left)
                        viewport.Center.X += (maxExtent.Left - viewport.Extent.Left) * cos;
                    if (viewport.Extent.Right > maxExtent.Right)
                        viewport.Center.X += (maxExtent.Right - viewport.Extent.Right) * cos;
                }
                if (MapHeightSpansViewport(maxExtent.Height, viewport.Height, viewport.Resolution)) // if it does't fit don't restrict
                {
                    //if ((viewport.Extent.Top> maxExtent.Top) && (viewport.Extent.Bottom < maxExtent.Bottom))
                    //    throw new Exception();
                    if (viewport.Extent.Top > maxExtent.Top)
                        viewport.Center.Y += (maxExtent.Top - viewport.Extent.Top) * sin;
                    if (viewport.Extent.Bottom < maxExtent.Bottom)
                        viewport.Center.Y += (maxExtent.Bottom - viewport.Extent.Bottom) * sin;
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
