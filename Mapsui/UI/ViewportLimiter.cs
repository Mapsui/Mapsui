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
        /// <summary>
        /// Restricts viewport in no way
        /// </summary>
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
        /// <summary>
        /// Restricts zoom in no way
        /// </summary>
        Unlimited,
        /// <summary>
        /// Restricts zoom of the viewport to ZoomLimits and, if ZoomLimits isn't 
        /// set, to minimum and maxiumum of Resolutions
        /// </summary>
        KeepWithinResolutions,
        /// <summary>
        /// Restricts zoom of the viewport to ZoomLimits and, if ZoomLimits isn't 
        /// set, to minimum and maxiumum of Resolutions, but fills always the
        /// complete viewport with map
        /// </summary>
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
            PanMode panMode, BoundingBox panLimits, BoundingBox mapEnvelope)
        {
            viewport.SetResolution(LimitResolution(viewport.Resolution, viewport.Width, viewport.Height, zoomMode, zoomLimits, mapResolutions, mapEnvelope));
            LimitExtent(viewport, panMode, panLimits, mapEnvelope);
        }

        public static double LimitResolution(double resolution, double screenWidth, double screenHeight, ZoomMode zoomMode, MinMax zoomLimits, 
            IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope)
        {
            if (zoomMode == ZoomMode.Unlimited) return resolution;

            var resolutionExtremes = zoomLimits ?? GetExtremes(mapResolutions);
            if (resolutionExtremes == null) return resolution;

            if (zoomMode == ZoomMode.KeepWithinResolutions)
            {
                if (resolutionExtremes.Min > resolution) return resolutionExtremes.Min;
                if (resolutionExtremes.Max < resolution) return resolutionExtremes.Max;
            }
            else if (zoomMode == ZoomMode.KeepWithinResolutionsAndAlwaysFillViewport)
            {
                if (resolutionExtremes.Min > resolution) return resolutionExtremes.Min;
                
                // This is the ...AndAlwaysFillViewport part
                var viewportFillingResolution = CalculateResolutionAtWhichMapFillsViewport(screenWidth, screenHeight, mapEnvelope);
                if (viewportFillingResolution < resolutionExtremes.Min) return resolution; // Mission impossible. Can't adhere to both restrictions
                var limit = Math.Min(resolutionExtremes.Max, viewportFillingResolution);
                if (limit < resolution) return limit;
            }

            return resolution;
        }

        private static double CalculateResolutionAtWhichMapFillsViewport(double screenWidth, double screenHeight, BoundingBox mapEnvelope)
        {
            return Math.Min(mapEnvelope.Width / screenWidth, mapEnvelope.Height / screenHeight);
        }

        public static void LimitExtent(IViewport viewport, PanMode panMode, BoundingBox panLimits, BoundingBox mapEnvelope)
        {
            var maxExtent = panLimits ?? mapEnvelope;
            if (maxExtent == null)
            {
                // Can be null because both panLimits and Map.Extent can be null. 
                // The Map.Extent can be null if the extent of all layers is null
                return; 
            }

            if (panMode == PanMode.KeepCenterWithinExtents)
            {
                var x = viewport.Center.X;
                if (viewport.Center.X < maxExtent.Left)  x = maxExtent.Left;
                if (viewport.Center.X > maxExtent.Right) x = maxExtent.Right;

                var y = viewport.Center.Y;
                if (viewport.Center.Y > maxExtent.Top) y = maxExtent.Top;
                if (viewport.Center.Y < maxExtent.Bottom) y = maxExtent.Bottom;

                viewport.SetCenter(x, y);
            }
            else if (panMode == PanMode.KeepViewportWithinExtents)
            {
                var x = viewport.Center.X;

                if (MapWidthSpansViewport(maxExtent.Width, viewport.Width, viewport.Resolution)) // if it does't fit don't restrict
                {
                    //if ((viewport.Extent.Left < maxExtent.Left) && (viewport.Extent.Right > maxExtent.Right))
                    //    throw new Exception();
                    if (viewport.Extent.Left < maxExtent.Left)
                        x  += maxExtent.Left - viewport.Extent.Left;
                    if (viewport.Extent.Right > maxExtent.Right)
                        x += maxExtent.Right - viewport.Extent.Right;
                }

                var y = viewport.Center.Y;
                if (MapHeightSpansViewport(maxExtent.Height, viewport.Height, viewport.Resolution)) // if it does't fit don't restrict
                {
                    //if ((viewport.Extent.Top> maxExtent.Top) && (viewport.Extent.Bottom < maxExtent.Bottom))
                    //    throw new Exception();
                    if (viewport.Extent.Top > maxExtent.Top)
                        y += maxExtent.Top - viewport.Extent.Top;
                    if (viewport.Extent.Bottom < maxExtent.Bottom)
                        y += maxExtent.Bottom - viewport.Extent.Bottom;
                }
                viewport.SetCenter(x, y);
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
