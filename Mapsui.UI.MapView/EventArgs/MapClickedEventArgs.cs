using Mapsui.Manipulations;
using System;

namespace Mapsui.UI.Maui;

public sealed class MapClickedEventArgs : EventArgs
{
    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    public Position Point { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    public TapType TapType { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    public MapClickedEventArgs(Position point, TapType tapType)
    {
        Point = point;
        TapType = tapType;
    }
}
