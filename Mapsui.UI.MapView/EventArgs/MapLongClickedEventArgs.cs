using System;

namespace Mapsui.UI.Maui;

public sealed class MapLongClickedEventArgs : EventArgs
{
    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    public Position Point { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    public bool Handled { get; set; } = false;

    internal MapLongClickedEventArgs(Position point)
    {
        Point = point;
    }
}
