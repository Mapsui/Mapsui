﻿using System;

namespace Mapsui.UI;

public sealed class MapClickedEventArgs : EventArgs, IMapClicked
{
    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    public Position Point { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    public int NumOfTaps { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    public MapClickedEventArgs(Position point, int numOfTaps)
    {
        Point = point;
        NumOfTaps = numOfTaps;
    }
}
