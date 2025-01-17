// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Mapsui.Utilities;

public static class ZoomHelper
{
    public static double GetResolutionToZoomIn(IReadOnlyList<double>? resolutions, double resolution)
    {
        var newResolution = resolution / 2.0;

        if (resolutions == null || resolutions.Count == 0)
            return newResolution; // No snapping possible. Return as calculated.

        for (var i = 0; i < resolutions.Count; i++)
        {
            // Is there a snap resolution smaller or equal?
            if (resolutions[i] <= (newResolution - double.Epsilon))
            {
                return resolutions[i];
            }
        }

        // No snapping, return as calculated.
        return newResolution;
    }

    public static double GetResolutionToZoomOut(IReadOnlyList<double>? resolutions, double resolution)
    {
        var newResolution = resolution * 2.0;

        if (resolutions == null || resolutions.Count == 0)
            return newResolution;  // No snapping possible. Return as calculated.

        for (var i = resolutions.Count - 1; i >= 0; i--)
        {
            // Is there a snap resolution bigger or equal?
            if (resolutions[i] >= (newResolution + double.Epsilon))
            {
                return resolutions[i];
            }
        }

        // No snapping, return as calculated.
        return newResolution;
    }

    public static double CalculateResolutionForWorldSize(double worldWidth, double worldHeight, double screenWidth,
        double screenHeight, MBoxFit boxFit = MBoxFit.Fit)
    {
        var widthResolution = worldWidth / screenWidth;
        var heightResolution = worldHeight / screenHeight;

        switch (boxFit)
        {
            case MBoxFit.FitHeight:
                return heightResolution;
            case MBoxFit.FitWidth:
                return widthResolution;
            case MBoxFit.Fill:
                return Math.Min(widthResolution, heightResolution);
            case MBoxFit.Fit:
                return Math.Max(widthResolution, heightResolution);
            default:
                throw new Exception("BoxFit not supported");
        }
    }
}
