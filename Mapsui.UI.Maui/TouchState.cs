using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.UI.Blazor;
internal record TouchState(MPoint Center, double Radius, double Angle, TouchMode Mode)
{
    public static TouchState FromLocations(List<MPoint> locations)
    {
        double centerX = 0;
        double centerY = 0;
        double radius = 0;
        double angle = 0;

        if (locations.Count > 0)
        {
            centerX = locations.Average(l => l.X);
            centerY = locations.Average(l => l.Y);

            if (locations.Count >= 2)
            {
                radius = Algorithms.Distance(locations[0].X, locations[0].Y, locations[1].X, locations[1].Y);
                angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;
            }
        }
        return new TouchState(new MPoint(centerX, centerY), radius, angle,  ToTouchMode(locations));
    }

    public static TouchMode ToTouchMode(List<MPoint> locations)
    {
        if (locations.Count == 0) return TouchMode.None;
        if (locations.Count == 2) return TouchMode.Zooming;
        return TouchMode.Dragging;        
    }
}
