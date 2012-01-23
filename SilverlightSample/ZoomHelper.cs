// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

namespace BruTile.UI
{
    //todo: This class belongs in a BruTile.UI assembly but since it is currently one fo the few 
    //classes that belongs there it would be overkill to create such an assembly
    public static class ZoomHelper
    {
        public static double ZoomIn(IList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0)
            {
                throw new ArgumentException("No tile resolutions");
            }

            //smaller than smallest
            if (resolutions[resolutions.Count - 1] > resolution) return resolutions[resolutions.Count - 1];

            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i] < resolution)
                    return resolutions[i];
            }
            return resolutions[resolutions.Count - 1];
        }

        public static double ClipToExtremes(IList<double> resolutions, double resolution)
        {
            //smaller than smallest
            if (resolutions[resolutions.Count - 1] > resolution) return resolutions[resolutions.Count - 1];

            //bigger than biggest
            if (resolutions[0] < resolution) return resolutions[0];

            return resolution;
        }

        public static double ZoomOut(IList<double> resolutions, double resolution)
        {
            if (resolutions.Count == 0)
            {
                throw new ArgumentException("No tile resolutions");
            }

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
    }
}
