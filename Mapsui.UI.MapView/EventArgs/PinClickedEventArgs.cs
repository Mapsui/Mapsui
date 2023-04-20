﻿using System;

#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#else
namespace Mapsui.UI;
#endif

public sealed class PinClickedEventArgs : EventArgs
{
    /// <summary>
    /// Pin that was clicked
    /// </summary>
    public IPin Pin { get; }

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

    internal PinClickedEventArgs(IPin pin, Position point, int numOfTaps)
    {
        Pin = pin;
        Point = point;
        NumOfTaps = numOfTaps;
    }
}
