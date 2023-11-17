using System;
using Microsoft.Maui.Graphics;

namespace Mapsui.UI.Maui;

public sealed class DrawableClickedEventArgs : EventArgs
{
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
    public int NumOfTaps { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    internal DrawableClickedEventArgs(Position point, Point screenPoint, int numOfTaps)
    {
        Point = point;
        ScreenPoint = screenPoint;
        NumOfTaps = numOfTaps;
    }
}
