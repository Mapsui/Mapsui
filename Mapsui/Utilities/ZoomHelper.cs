// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System;
using System.Collections.Generic;

namespace Mapsui.Utilities;

public static class ZoomHelper
{
    public static double GetResolutionToZoomIn(IReadOnlyList<double>? resolutions, double resolution)
    {
        if (resolutions == null || resolutions.Count == 0) return resolution / 2.0;

        foreach (var t in resolutions)
        {
            // If there is a smaller resolution in the array return it
            if (t < resolution - double.Epsilon) return t;
        }

        // Else return half of the current resolution
        return resolution / 2.0;
    }

    public static double GetResolutionToZoomOut(IReadOnlyList<double>? resolutions, double resolution)
    {
        if (resolutions == null || resolutions.Count == 0) return resolution * 2.0;

        for (var i = resolutions.Count - 1; i >= 0; i--)
        {
            // If there is a bigger resolution in the array return it
            if (resolutions[i] > (resolution + double.Epsilon)) return resolutions[i];
        }

        // Else return double the current resolution
        return resolution * 2.0;
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
