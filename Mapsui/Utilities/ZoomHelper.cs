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

using System.Collections.Generic;

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
            if (resolutions[resolutions.Count - 1] > resolution) return resolutions[resolutions.Count - 1];

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
                if (resolutions[i] > resolution)
                    return resolutions[i];
            }
            return resolutions[0];
        }

        public static void ZoomToBoudingbox(double xMin, double yMin, double xMax, double yMax, double screenWidth, out double x, out double y, out double resolution)
        {
            if (xMin > xMax)//User dragged from right to left
            {
                double tempX = xMin;
                double tempY = yMin;
                xMin = xMax;
                yMin = yMax;
                xMax = tempX;
                yMax = tempY;
            }

            x = (xMax + xMin) / 2;
            y = (yMax + yMin) / 2;
            resolution = (xMax - xMin) / screenWidth;
        }

        public static void ZoomToBoudingbox(Viewport viewport, double xMin, double yMin, double xMax, double yMax, double screenWidth)
        {
            if (xMin > xMax)//User dragged from right to left
            {
                double tempX = xMin;
                double tempY = yMin;
                xMin = xMax;
                yMin = yMax;
                xMax = tempX;
                yMax = tempY;
            }

            var x = (xMax + xMin) / 2;
            var y = (yMax + yMin) / 2;
            var resolution = (xMax - xMin) / screenWidth;
            viewport.CenterX = x;
            viewport.CenterY = y;
            viewport.Resolution = resolution;
        }
    }
}
