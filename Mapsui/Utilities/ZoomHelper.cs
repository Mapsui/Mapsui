// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;

namespace Mapsui.Utilities
{
    public static class ZoomHelper
    {
        public static double ZoomIn(IReadOnlyList<double> resolutions, double resolution)
        {
            if (resolutions == null || resolutions.Count == 0) return resolution / 2.0;

            foreach (var r in resolutions)
                if (r < resolution) return r;
            return resolutions[resolutions.Count - 1];
        }
        
        public static double ZoomOut(IReadOnlyList<double> resolutions, double resolution)
        {
            if (resolutions == null || resolutions.Count == 0) return resolution * 2.0;

            for (var i = resolutions.Count - 1; i >= 0; i--)
                if (resolutions[i] > resolution) return resolutions[i];
            return resolutions[0];
        }

        [Obsolete("Use ViewportLimiter.LimitExtent instead")]
        public static double ClipResolutionToExtremes(IReadOnlyList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0) return resolution;

            // smaller than smallest
            if (resolutions[resolutions.Count - 1] > resolution) return resolutions[resolutions.Count - 1];

            // bigger than biggest
            if (resolutions[0] < resolution) return resolutions[0];

            return resolution;
        }

        public static double DetermineResolution(double worldWidth, double worldHeight, double screenWidth,
            double screenHeight, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            var widthResolution = worldWidth/screenWidth;
            var heightResolution = worldHeight/screenHeight;

            switch (scaleMethod)
            {
                case ScaleMethod.FitHeight:
                    return heightResolution;
                case ScaleMethod.FitWidth:
                    return widthResolution;
                case ScaleMethod.Fill:
                    return Math.Min(widthResolution, heightResolution);
                case ScaleMethod.Fit:
                    return Math.Max(widthResolution, heightResolution);
                default:
                    throw new Exception("ScaleMethod not supported");
            }
        }

        public static void ZoomToBoudingbox(double x1, double y1, double x2, double y2,
            double screenWidth, double screenHeight,
            out double x, out double y, out double resolution,
            ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            if (x1 > x2) Swap(ref x1, ref x2);
            if (y1 > y2) Swap(ref y1, ref y2);

            x = (x2 + x1)/2;
            y = (y2 + y1)/2;

            if (scaleMethod == ScaleMethod.Fit)
                resolution = Math.Max((x2 - x1) / screenWidth, (y2 - y1) / screenHeight);
            else if (scaleMethod == ScaleMethod.Fill)
                resolution = Math.Min((x2 - x1) / screenWidth, (y2 - y1) / screenHeight);
            else if (scaleMethod == ScaleMethod.FitWidth)
                resolution = (x2 - x1) / screenWidth;
            else if (scaleMethod == ScaleMethod.FitHeight)
                resolution = (y2 - y1) / screenHeight;
            else
                throw new Exception("FillMethod not found");
        }

        public static void ZoomToBoudingbox(Viewport viewport,
            double x1, double y1, double x2, double y2,
            double screenWidth, double screenHeight,
            ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            ZoomToBoudingbox(x1, y1, x2, y2, screenWidth, screenHeight,
                out var centerX, out var centerY, out var resolution, scaleMethod);

            viewport.SetCenter(centerX, centerY);

            viewport.Resolution = resolution;
        }
        
        private static void Swap(ref double xMin, ref double xMax)
        {
            var tempX = xMin;
            xMin = xMax;
            xMax = tempX;
        }
    }

    public enum ScaleMethod
    {
        /// <summary>
        ///     Fit within the view port of the screen
        /// </summary>
        Fit,

        /// <summary>
        ///     Fill up the entire view port of the screen
        /// </summary>
        Fill,

        /// <summary>
        ///     Fill the width of the screen
        /// </summary>
        FitWidth,

        /// <summary>
        ///     Fill the height of the screen
        /// </summary>
        FitHeight
    }
}