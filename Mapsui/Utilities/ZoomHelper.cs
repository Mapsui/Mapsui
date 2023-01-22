// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System;
using System.Collections.Generic;

namespace Mapsui.Utilities;

public static class ZoomHelper
{
    public static double ZoomIn(IReadOnlyList<double>? resolutions, double resolution)
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

    public static double ZoomOut(IReadOnlyList<double>? resolutions, double resolution)
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
        var widthResolution = worldWidth / screenWidth;
        var heightResolution = worldHeight / screenHeight;

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

        x = (x2 + x1) / 2;
        y = (y2 + y1) / 2;

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
        viewport.SetResolution(resolution);
    }

    private static void Swap(ref double xMin, ref double xMax)
    {
        (xMin, xMax) = (xMax, xMin);
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
