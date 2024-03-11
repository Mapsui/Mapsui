using System;

namespace Mapsui;

public class MapInfoEventArgs : EventArgs
{
    public MapInfo? MapInfo { get; init; }

    /// <summary>
    /// Number of times the user tapped the location
    /// </summary>
    public int TapType { get; init; }

    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; }
}
