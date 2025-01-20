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
            if (resolutions[i] > (newResolution - double.Epsilon))
                continue; // Ignore bigger snap resolutions

            // The resolutions increase in multiples of two, so we need to take log2 for the difference comparison.
            if (Math.Log2(newResolution) - Math.Log2(resolutions[i]) < 0.5)
                return resolutions[i];
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
            if (resolutions[i] < (newResolution + double.Epsilon))
                continue; // Ignore smaller snap resolutions

            // The resolutions increase in multiples of two, so we need to take log2 for the difference comparison.
            if (Math.Log2(resolutions[i] - Math.Log2(newResolution)) < 0.5)
                return resolutions[i];
        }

        // No snapping, return as calculated.
        return newResolution;
    }

    public static double CalculateResolutionForWorldSize(double worldWidth, double worldHeight, double screenWidth,
        double screenHeight, MBoxFit boxFit = MBoxFit.Fit)
    {
        var widthResolution = worldWidth / screenWidth;
        var heightResolution = worldHeight / screenHeight;

        return boxFit switch
        {
            MBoxFit.FitHeight => heightResolution,
            MBoxFit.FitWidth => widthResolution,
            MBoxFit.Fill => Math.Min(widthResolution, heightResolution),
            MBoxFit.Fit => Math.Max(widthResolution, heightResolution),
            _ => throw new Exception("BoxFit not supported"),
        };
    }
}
