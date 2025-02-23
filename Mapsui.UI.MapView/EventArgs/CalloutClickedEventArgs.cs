﻿using System;
using Mapsui.Manipulations;
using Microsoft.Maui.Graphics;

namespace Mapsui.UI.Maui;

public sealed class CalloutClickedEventArgs : EventArgs
{
    /// <summary>
    /// Callout that is clicked
    /// </summary>
    public Callout? Callout { get; }

    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    public Position Point { get; }

    /// <summary>
    /// Point of click in screen coordinates
    /// </summary>
    public Point ScreenPoint { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    public GestureType GestureType { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    internal CalloutClickedEventArgs(Callout? callout, Position point, Point screenPoint, GestureType gestureType)
    {
        Callout = callout;
        Point = point;
        ScreenPoint = screenPoint;
        GestureType = gestureType;
    }
}
