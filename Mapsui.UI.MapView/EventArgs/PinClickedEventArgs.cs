﻿using Mapsui.Manipulations;
using System;

namespace Mapsui.UI.Maui;

public sealed class PinClickedEventArgs : EventArgs
{
    /// <summary>
    /// Pin that was clicked
    /// </summary>
    public Pin Pin { get; }

    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    public Position Point { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    public GestureType GestureType { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    internal PinClickedEventArgs(Pin pin, Position point, GestureType gestureType)
    {
        Pin = pin;
        Point = point;
        GestureType = gestureType;
    }
}
