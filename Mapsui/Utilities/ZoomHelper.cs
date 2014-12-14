// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Utilities
{
    public static class ZoomHelper
    {
        public static double ZoomIn(IList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0) return resolution / 2.0;
            
            //smaller than smallest
            if (resolutions[resolutions.Count - 1] > resolution) return resolutions[resolutions.Count - 1];

            foreach (double resolutionOfLevel in resolutions)
            {
                if (resolutionOfLevel < resolution)
                    return resolutionOfLevel;
            }
            return resolutions[resolutions.Count - 1];
        }

        public static double ClipToExtremes(IList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0) return resolution;

            //smaller than smallest
            if (resolutions[resolutions.Count - 1] > (resolution + 0.2 * resolution)) return resolutions[resolutions.Count - 1];

            //bigger than biggest
            if (resolutions[0] < resolution) return resolutions[0];

            return resolution;
        }

        public static double ZoomOut(IList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0) return resolution * 2.0;

            //bigger than biggest
            if (resolutions[0] < resolution) return resolutions[0];

            for (int i = resolutions.Count - 1; i >= 0; i--)
            {
                if (resolutions[i] > (resolution + 0.2 * resolution))
                    return resolutions[i];
            }
            return resolutions[0];
        }

        public static double DetermineResolution(double worldWidth, double worldHeight, double screenWidth, double screenHeight, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            double widthResolution = worldWidth / screenWidth;
            double heightResolution = worldHeight/screenHeight;
            switch (scaleMethod)
            {
                case ScaleMethod.FitHeight:
                    return heightResolution;
                case ScaleMethod.FitWidth:
                    return widthResolution;
                case ScaleMethod.Fill:
                    return Math.Max(widthResolution, heightResolution);
                case ScaleMethod.Fit:
                    return Math.Min(widthResolution, heightResolution);
                default:
                    throw new Exception("ScaleMethod not supported");
            }
        }

        public static void ZoomToBoudingbox(double x1, double y1, double x2, double y2, double screenWidth, out double x, out double y, out double resolution)
        {
            if (x1 > x2) Swap(ref x1, ref x2);
            if (y1 > y2) Swap(ref y1, ref y2);

            x = (x2 + x1) / 2;
            y = (y2 + y1) / 2;
            resolution = (x2 - x1) / screenWidth;
        }

        public static void ZoomToBoudingbox(Viewport viewport, double x1, double y1, double x2, double y2, double screenWidth)
        {
            if (x1 > x2) Swap(ref x1, ref x2);
            if (y1 > y2) Swap(ref y1, ref y2);

            var x = (x2 + x1) / 2;
            var y = (y2 + y1) / 2;
            var resolution = (x2 - x1) / screenWidth;
            viewport.Center.X = x;
            viewport.Center.Y = y;
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
        FitWidth,
        FitHeight,
        Fill,
        Fit
    }
}
